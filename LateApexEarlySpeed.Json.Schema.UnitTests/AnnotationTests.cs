using LateApexEarlySpeed.Json.Schema.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class AnnotationTests
{
    private static readonly string[] UnsupportedFileNames = new[] { "unevaluated", "unknown" };

    [Theory]
    [MemberData(nameof(AnnotationTestSuite))]
    public void Validate_InputFromJsonSchemaTestSuite(DialectKind dialect, string schema, string instance, string keyword, string instanceLocation, Dictionary<string, JsonElement> expectedAnnotations)
    {
        var jsonValidator = new JsonValidator(schema, new JsonValidatorOptions { CollectAnnotations = true, DefaultDialect = dialect });
        ValidationResult validationResult = jsonValidator.Validate(instance, new JsonSchemaOptions { OutputFormat = OutputFormat.List, ValidateFormat = true });

        Assert.True(validationResult.IsValid);

        LinkedListBasedImmutableJsonPointer? requestedInstanceLocation = LinkedListBasedImmutableJsonPointer.Create(instanceLocation);
        Dictionary<LinkedListBasedImmutableJsonPointer, JsonElement> actualAnnotationInfos = validationResult.Annotations.Where(a => a.Keyword == keyword && a.InstanceLocation.Equals(requestedInstanceLocation))
            .ToDictionary(a =>
            {
                string fragment = a.SubSchemaRefFullUri.UnescapedFragmentWithoutNumberSign();

                if (string.IsNullOrEmpty(fragment))
                {
                    LinkedListBasedImmutableJsonPointer actualSchemaPath = LinkedListBasedImmutableJsonPointer.Empty;

                    foreach (string token in a.RelativeKeywordLocation.SkipLast(1)) // because a's RelativeKeywordLocation is like /.../keywordName
                    {
                        actualSchemaPath = actualSchemaPath.Add(token);
                    }

                    return actualSchemaPath;
                }
                else // in current test suite, this means there is reference keyword
                {
                    LinkedListBasedImmutableJsonPointer actualSchemaPath = LinkedListBasedImmutableJsonPointer.Create(fragment)!;

                    foreach (string token in a.RelativeKeywordLocation.Skip(1).SkipLast(1)) // because a's RelativeKeywordLocation is like /$ref/.../keywordName
                    {
                        actualSchemaPath = actualSchemaPath.Add(token);
                    }

                    return actualSchemaPath;
                }
            }, a => a.Value);

        Dictionary<LinkedListBasedImmutableJsonPointer, JsonElement> expectedAnnotationInfos = expectedAnnotations.ToDictionary(a
            => LinkedListBasedImmutableJsonPointer.Create(Uri.UnescapeDataString(a.Key.TrimStart('#')))!, a => a.Value);

        Assert.Equal(expectedAnnotationInfos.Count, actualAnnotationInfos.Count);

        foreach (KeyValuePair<LinkedListBasedImmutableJsonPointer, JsonElement> expectedAnnotationInfo in expectedAnnotationInfos)
        {
            JsonElement actualValue = Assert.Contains(expectedAnnotationInfo.Key, (IDictionary<LinkedListBasedImmutableJsonPointer, JsonElement>)actualAnnotationInfos);

            Assert.Equal(JsonSerializer.Serialize(expectedAnnotationInfo.Value), JsonSerializer.Serialize(actualValue));
        }
    }

    public static IEnumerable<object[]> AnnotationTestSuite
    {
        get
        {
            TestCase[] annotationCases = TestSuiteReader.ReadAnnotationTestCasesFromJsonSchemaTestSuite(UnsupportedFileNames);

            return annotationCases.SelectMany(annotationCase
                => annotationCase.Tests.SelectMany(testCase
                    => testCase.Assertions.SelectMany(assertion
                        => annotationCase.Dialects.Select(dialect 
                            => new object[] { dialect, JsonSerializer.Serialize(annotationCase.Schema), JsonSerializer.Serialize(testCase.Instance), assertion.Keyword, assertion.InstanceLocation, assertion.Expected })
                        )));
        }
    }

    /// <summary>
    /// Refer to: https://github.com/json-schema-org/JSON-Schema-Test-Suite/tree/main/annotations#test-case-components
    /// </summary>
    private class TestCase
    {
        private static readonly Dictionary<string, int> CompatibilityLevelTable = new()
        {
            { "3", 0 },
            { "4", 0 },
            { "6", 0 },
            { "7", 0 },
            { "2019", 1 },
            { "2020", 2 }
        };

        private static readonly DialectKind[] AvailableDialectKinds = new[] {
            DialectKind.Draft7,
            DialectKind.Draft201909,
            DialectKind.Draft202012
        };

        [JsonPropertyName("compatibility")]
        public string Compatibility { get; set; } = "3"; // If no compatibility is specified, it is assumed to support any dialects.

        [JsonPropertyName("schema")]
        public JsonElement Schema { get; set; }
        
        [JsonPropertyName("tests")]
        public Test[] Tests { get; set; } = null!;

        public IEnumerable<DialectKind> Dialects
        {
            get
            {
                int compatibilityLevel = CompatibilityLevelTable[Compatibility];

                return AvailableDialectKinds.Skip(compatibilityLevel);
            }
        }
    }

    /// <summary>
    /// Refer to: https://github.com/json-schema-org/JSON-Schema-Test-Suite/tree/main/annotations#test-components
    /// </summary>
    private class Test
    {
        [JsonPropertyName("instance")]
        public JsonElement Instance { get; set; }
        
        [JsonPropertyName("assertions")]
        public Assertion[] Assertions { get; set; } = null!;
    }

    /// <summary>
    /// Refer to: https://github.com/json-schema-org/JSON-Schema-Test-Suite/tree/main/annotations#assertions-components
    /// </summary>
    private class Assertion
    {
        [JsonPropertyName("location")]
        public string InstanceLocation { get; set; } = null!;

        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = null!;

        [JsonPropertyName("expected")]
        public Dictionary<string, JsonElement> Expected { get; set; } = null!;
    }

    private static class TestSuiteReader
    {
        public static TestCase[] ReadAnnotationTestCasesFromJsonSchemaTestSuite(string[] unsupportedFileNames)
        {
            string[] pathFiles = Directory.GetFiles(Path.Combine("JSON-Schema-Test-Suite", "annotations", "tests"));

            IEnumerable<TestCase> result = Enumerable.Empty<TestCase>();

            foreach (string pathFile in pathFiles)
            {
                if (IsUnsupportedFile(pathFile, unsupportedFileNames))
                {
                    continue;
                }

                result = result.Concat(ReadTestCases(pathFile));
            }

            return result.ToArray();
        }

        private static IEnumerable<TestCase> ReadTestCases(string pathFile)
        {
            using (FileStream fs = File.OpenRead(pathFile))
            {
                using (JsonDocument rootDoc = JsonDocument.Parse(fs))
                {
                    JsonElement suites = rootDoc.RootElement.GetProperty("suite");

                    return suites.Deserialize<TestCase[]>()!;
                }
            }
        }

        private static bool IsUnsupportedFile(string pathFile, string[] unsupportedFileNames)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathFile);
            return unsupportedFileNames.Contains(fileNameWithoutExtension);
        }
    }
}
