using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace LateApexEarlySpeed.Json.Schema.UnitTests
{
    public class JsonValidatorTest_ByJsonSchemaTestSuite : IClassFixture<ExternalSchemaDocumentsFixture>
    {
        private static readonly string[] UnsupportedTestFiles = new[] { "unevaluatedItems", "unevaluatedProperties", "uniqueItems", "enum", "const", "vocabulary" };
        private static readonly string[] UnsupportedTestCases = new[]
        {
            "invalid anchors", 
            "validate definition against metaschema", 
            "Invalid use of fragments in location-independent $id",
            "Valid use of empty fragments in location-independent $id", 
            "Unnormalized $ids are allowed but discouraged", 
            "remote ref, containing refs itself", 
            "URN base URI with f-component",

            "$anchor inside an enum is not a real identifier",
            "$id inside an enum is not a real identifier",
            "non-schema object containing an $anchor property",
            "non-schema object containing an $id property",
            "contains keyword with const keyword",
            "minContains=2 with contains",
            "maxContains with contains",
            "minContains=1 with contains",
            "if with boolean schema true",
            "if appears at the end when serialized (keyword processing sequence)",
            "if appears at the end when serialized (keyword processing sequence)",
            "if with boolean schema false",
            "naive replacement of $ref with its destination is not correct",
            "collect annotations inside a 'not', even if collection is disabled",
            "strict-tree schema, guards against misspelled properties",
            "root pointer ref",
            "Recursive references between schemas",
            "ref creates new scope when adjacent to keywords",
            "A $dynamicRef that initially resolves to a schema with a matching $dynamicAnchor resolves to the first $dynamicAnchor in the dynamic scope",
            "multiple dynamic paths to the $dynamicRef keyword",

        };
        
        private readonly IEnumerable<string> _externalSchemaDocuments;
        private readonly ITestOutputHelper _testOutputHelper;

        public JsonValidatorTest_ByJsonSchemaTestSuite(ExternalSchemaDocumentsFixture externalSchemaDocumentsFixture, ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _externalSchemaDocuments = externalSchemaDocumentsFixture.ExternalSchemaDocuments;
        }

        [Theory]
        [MemberData(nameof(JsonSchemaTestSuiteForDraft2020))]
        public void Validate_InputFromJsonSchemaTestSuite(string schema, string instance, bool expectedValidationResult, string testCaseDescription, string testDescription)
        {
            _testOutputHelper.WriteLine($"Test case description: {testCaseDescription}");
            _testOutputHelper.WriteLine($"Test description: {testDescription}");

            var jsonValidator = new JsonValidator(schema);
            foreach (string schemaDocument in _externalSchemaDocuments)
            {
                jsonValidator.AddExternalDocument(schemaDocument);
            }
            
            Assert.Equal(expectedValidationResult, jsonValidator.Validate(instance).IsValid);
        }

        public static IEnumerable<object[]> JsonSchemaTestSuiteForDraft2020
        {
            get
            {
                IEnumerable<TestCase> testCases = TestSuiteReader.ReadTestCases("draft2020-12", UnsupportedTestFiles, UnsupportedTestCases);

                foreach (TestCase testCase in testCases)
                {
                    foreach (Test test in testCase.Tests)
                    {
                        yield return new object[]
                        {
                            JsonSerializer.Serialize(testCase.JsonSchema), 
                            JsonSerializer.Serialize(test.Instance), 
                            test.ValidationResult,
                            testCase.Description,
                            test.Description
                        };
                    }
                }
            }
        }


        /// <summary>
        /// Refer to: https://github.com/json-schema-org/JSON-Schema-Test-Suite#terminology
        /// </summary>
        private class TestCase
        {
            public string Description { get; set; } = null!;

            [JsonPropertyName("schema")]
            public JsonElement JsonSchema { get; set; }

            [JsonPropertyName("tests")]
            public Test[] Tests { get; set; } = null!;
        }

        /// <summary>
        /// Refer to: https://github.com/json-schema-org/JSON-Schema-Test-Suite#terminology
        /// </summary>
        private class Test
        {
            public string Description { get; set; } = null!;

            [JsonPropertyName("data")]
            public JsonElement Instance { get; set; }

            [JsonPropertyName("valid")]
            public bool ValidationResult { get; set; }
        }

        private static class TestSuiteReader
        {
            public static IEnumerable<TestCase> ReadTestCases(string draftVersion, string[] unsupportedKeywords, string[] unsupportedTestCases)
            {
                string[] pathFiles = Directory.GetFiles(Path.Combine("JSON-Schema-Test-Suite", "tests", draftVersion));
                
                foreach (string pathFile in pathFiles)
                {
                    if (IsFileForUnsupportedKeyword(pathFile, unsupportedKeywords))
                    {
                        continue;
                    }

                    using (FileStream fs = File.OpenRead(pathFile))
                    {
                        TestCase[] testCases = JsonSerializer.Deserialize<TestCase[]>(fs, new JsonSerializerOptions{PropertyNameCaseInsensitive = true})!;
                        foreach (TestCase testCase in testCases)
                        {
                            if (!IsUnsupportedTestCase(testCase, unsupportedTestCases))
                            {
                                yield return testCase;
                            }
                        }
                    }
                }
            }

            private static bool IsUnsupportedTestCase(TestCase testCase, string[] unsupportedTestCases)
            {
                return unsupportedTestCases.Contains(testCase.Description);
            }

            private static bool IsFileForUnsupportedKeyword(string pathFile, string[] unsupportedKeywords)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathFile);
                return unsupportedKeywords.Contains(fileNameWithoutExtension);
            }
        }
    }
}