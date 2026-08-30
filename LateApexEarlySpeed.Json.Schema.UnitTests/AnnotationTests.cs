using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class AnnotationTests
{
    private static readonly string[] UnsupportedFileNames = new[] { "unevaluated", "unknown" };

    [Theory]
    [MemberData(nameof(AnnotationTestSuite))]
    public void Validate_InputFromJsonSchemaTestSuite(string schema, string instance, string keyword, string instanceLocation, Dictionary<string, JsonElement> expectedAnnotations)
    {
        var jsonValidator = new JsonValidator(schema, new JsonValidatorOptions { CollectAnnotations = true });
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
            AnnotationTestCase[] annotationCases = TestSuiteReader.ReadAnnotationTestCasesFromJsonSchemaTestSuite(UnsupportedFileNames);

            return annotationCases.SelectMany(annotationCase
                => annotationCase.Tests.SelectMany(testCase
                    => testCase.Assertions.Select(assertion
                        => new object[] { JsonSerializer.Serialize(annotationCase.Schema), JsonSerializer.Serialize(testCase.Instance), assertion.Keyword, assertion.InstanceLocation, assertion.Expected })));
        }
    }

    public class Assertion : IComparable<Assertion>
    {
        public Assertion(string keyword, ImmutableJsonPointer keywordLocation, ImmutableJsonPointer instanceLocation, JsonElement annotationValue)
        {
            Keyword = keyword;
            KeywordLocation = keywordLocation;
            InstanceLocation = instanceLocation;
            AnnotationValue = annotationValue;
        }

        public string Keyword { get; }
        public ImmutableJsonPointer KeywordLocation { get; }
        public ImmutableJsonPointer InstanceLocation { get; }
        public JsonElement AnnotationValue { get; }

        public int CompareTo(Assertion? other)
        {
            Debug.Assert(other is not null);
            
            string thisKey = Keyword + KeywordLocation + InstanceLocation + JsonSerializer.Serialize(AnnotationValue);
            string otherKey = other.Keyword + other.KeywordLocation + other.InstanceLocation + JsonSerializer.Serialize(other.AnnotationValue);
        
            return string.Compare(thisKey, otherKey, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Assertion)obj);
        }

        protected bool Equals(Assertion other)
        {
            return Keyword == other.Keyword 
                   && KeywordLocation.Equals(other.KeywordLocation) 
                   && InstanceLocation.Equals(other.InstanceLocation) 
                   && JsonSerializer.Serialize(AnnotationValue) == JsonSerializer.Serialize(other.AnnotationValue);
        }

        public override int GetHashCode() => 0;
    }

    private class AnnotationTestCase
    {
        [JsonPropertyName("schema")]
        public JsonElement Schema { get; set; }
        
        [JsonPropertyName("tests")]
        public TestCase[] Tests { get; set; } = null!;
    }

    private class TestCase
    {
        [JsonPropertyName("instance")]
        public JsonElement Instance { get; set; }
        
        [JsonPropertyName("assertions")]
        public AssertionCase[] Assertions { get; set; } = null!;
    }

    private class AssertionCase
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
        public static AnnotationTestCase[] ReadAnnotationTestCasesFromJsonSchemaTestSuite(string[] unsupportedFileNames)
        {
            string[] pathFiles = Directory.GetFiles(Path.Combine("JSON-Schema-Test-Suite", "annotations", "tests"));

            IEnumerable<AnnotationTestCase> result = Enumerable.Empty<AnnotationTestCase>();

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

        private static IEnumerable<AnnotationTestCase> ReadTestCases(string pathFile)
        {
            using (FileStream fs = File.OpenRead(pathFile))
            {
                using (JsonDocument rootDoc = JsonDocument.Parse(fs))
                {
                    JsonElement suites = rootDoc.RootElement.GetProperty("suite");

                    return suites.Deserialize<AnnotationTestCase[]>()!;
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
