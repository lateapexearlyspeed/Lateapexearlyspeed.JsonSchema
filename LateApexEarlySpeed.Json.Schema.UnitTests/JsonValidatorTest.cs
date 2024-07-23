using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace LateApexEarlySpeed.Json.Schema.UnitTests
{
    public class JsonValidatorTest : IClassFixture<JsonValidatorTestFixture>
    {
        private static readonly string[] UnsupportedTestFiles = new[] { "unevaluatedItems", "unevaluatedProperties", "vocabulary" };
        private static readonly string[] UnsupportedTestCases = new[]
        {
            "collect annotations inside a 'not', even if collection is disabled",
            "strict-tree schema, guards against misspelled properties",
            "ref creates new scope when adjacent to keywords"
        };

        private static readonly string[] TestCasesDependOnRemoteHttpDocuments = new[]
        {
            "invalid anchors",
            "validate definition against metaschema",
            "Invalid use of fragments in location-independent $id",
            "Valid use of empty fragments in location-independent $id",
            "Unnormalized $ids are allowed but discouraged",
            "remote ref, containing refs itself",
            "URN base URI with f-component"
        };

        private readonly IEnumerable<string> _externalSchemaDocuments;
        private readonly Uri[] _httpBasedDocumentUris;
        private readonly ITestOutputHelper _testOutputHelper;

        public JsonValidatorTest(JsonValidatorTestFixture jsonValidatorTestFixture, ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _externalSchemaDocuments = jsonValidatorTestFixture.ExternalSchemaDocuments;
            _httpBasedDocumentUris = jsonValidatorTestFixture.HttpBasedDocumentUris;
        }

        [Theory]
        [MemberData(nameof(JsonSchemaTestSuiteForDraft2020))]
        public async Task ValidateByStringSchema_InputFromJsonSchemaTestSuite(string schema, string instance, bool expectedValidationResult, string testCaseDescription, string testDescription)
        {
            _testOutputHelper.WriteLine($"Test case description: {testCaseDescription}");
            _testOutputHelper.WriteLine($"Test description: {testDescription}");

            JsonValidator jsonValidator = await CreateJsonValidatorWithExternalDocumentSupportAsync(schema, testCaseDescription);

            Assert.Equal(expectedValidationResult, jsonValidator.Validate(instance, new JsonSchemaOptions{ValidateFormat = false}).IsValid);
        }

        [Theory]
        [MemberData(nameof(JsonSchemaTestSuiteForDraft2020))]
        public async Task GetStandardJsonSchemaText_InputFromJsonSchemaTestSuite(string schema, string instance, bool expectedValidationResult, string testCaseDescription, string testDescription)
        {
            _testOutputHelper.WriteLine($"Test case description: {testCaseDescription}");
            _testOutputHelper.WriteLine($"Test description: {testDescription}");

            // Prepare original jsonValidator
            JsonValidator jsonValidator = await CreateJsonValidatorWithExternalDocumentSupportAsync(schema, testCaseDescription);

            // Generate json schema text from jsonValidator
            string generatedSchemaText = jsonValidator.GetStandardJsonSchemaText();

            // Generate jsonValidator from previous generated json schema text
            jsonValidator = await CreateJsonValidatorWithExternalDocumentSupportAsync(generatedSchemaText, testCaseDescription);

            Assert.Equal(expectedValidationResult, jsonValidator.Validate(instance, new JsonSchemaOptions { ValidateFormat = false }).IsValid);
        }

        private async Task<JsonValidator> CreateJsonValidatorWithExternalDocumentSupportAsync(string schema, string testCaseDescription)
        {
            var jsonValidator = new JsonValidator(schema);
            foreach (string schemaDocument in _externalSchemaDocuments)
            {
                jsonValidator.AddExternalDocument(schemaDocument);
            }

            if (TestCasesDependOnRemoteHttpDocuments.Contains(testCaseDescription))
            {
                foreach (Uri remoteUri in _httpBasedDocumentUris)
                {
                    await jsonValidator.AddHttpDocumentAsync(remoteUri);
                }
            }

            return jsonValidator;
        }

        [Theory]
        [MemberData(nameof(JsonSchemaTestSuiteForDraft2020))]
        public async Task ValidateBySpanSchema_InputFromJsonSchemaTestSuite(string schema, string instance, bool expectedValidationResult, string testCaseDescription, string testDescription)
        {
            _testOutputHelper.WriteLine($"Test case description: {testCaseDescription}");
            _testOutputHelper.WriteLine($"Test description: {testDescription}");
        
            var jsonValidator = new JsonValidator(schema.AsSpan());
            foreach (string schemaDocument in _externalSchemaDocuments)
            {
                jsonValidator.AddExternalDocument(schemaDocument.AsSpan());
            }

            if (TestCasesDependOnRemoteHttpDocuments.Contains(testCaseDescription))
            {
                foreach (Uri remoteUri in _httpBasedDocumentUris)
                {
                    await jsonValidator.AddHttpDocumentAsync(remoteUri);
                }
            }

            Assert.Equal(expectedValidationResult, jsonValidator.Validate(instance, new JsonSchemaOptions{ValidateFormat = false}).IsValid);
        }

        [Theory]
        [MemberData(nameof(JsonSchemaTestCasesForFormatKeyword))]
        public void ValidateByStringSchema_ValidateFormatKeyword(string schema, string instance, bool expectedValidationResult, string testCaseDescription, string testDescription)
        {
            _testOutputHelper.WriteLine($"Test case description: {testCaseDescription}");
            _testOutputHelper.WriteLine($"Test description: {testDescription}");

            var jsonValidator = new JsonValidator(schema);

            Assert.Equal(expectedValidationResult, jsonValidator.Validate(instance).IsValid);
        }

        [Theory]
        [MemberData(nameof(JsonSchemaTestCasesForCustomFormat))]
        public void Validate_CustomFormatKeyword(string schema, string instance, bool expectedValidationResult, string testCaseDescription, string testDescription)
        {
            // 'custom_format' should have been registered during 'JsonValidatorTestFixture'
            Assert.NotNull(FormatRegistry.GetFormatType("custom_format"));

            var jsonValidator = new JsonValidator(schema);
            Assert.Equal(expectedValidationResult, jsonValidator.Validate(instance).IsValid);
        }

        /// <summary>
        /// These test cases are to validate https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues/45
        /// </summary>
        [Theory]
        [InlineData("2024-07-23T15:56:20+10:00")]
        [InlineData("2024-07-23T15:56:20.1+10:00")]
        [InlineData("2024-07-23T15:56:20.0+10:00")]
        [InlineData("2024-07-23T15:56:20.10+10:00")]
        [InlineData("2024-07-23T15:56:20.123+10:00")]
        [InlineData("2024-07-23T15:56:20.1234567+10:00")]
        [InlineData("2024-07-23T15:56:20-10:00")]
        [InlineData("2024-07-23T15:56:20.1-10:00")]
        [InlineData("2024-07-23T15:56:20.0-10:00")]
        [InlineData("2024-07-23T15:56:20.10-10:00")]
        [InlineData("2024-07-23T15:56:20.123-10:00")]
        [InlineData("2024-07-23T15:56:20.1234567-10:00")]
        [InlineData("2024-07-23T15:56:20Z")]
        [InlineData("2024-07-23T15:56:20.1Z")]
        [InlineData("2024-07-23T15:56:20.0Z")]
        [InlineData("2024-07-23T15:56:20.10Z")]
        [InlineData("2024-07-23T15:56:20.123Z")]
        [InlineData("2024-07-23T15:56:20.1234567Z")]
        public void Validate_ValidDateTimeFormat_ShouldPassValidation(string validDateTimeValue)
        {
            string schema = """
             {
               "type": "string",
               "format": "date-time"
             }
             """;

            var jsonValidator = new JsonValidator(schema);

            string jsonInstance = $"\"{validDateTimeValue}\"";
            Assert.True(jsonValidator.Validate(jsonInstance).IsValid);
        }

        [Theory]
        [InlineData("15:56:20+10:00")]
        [InlineData("15:56:20.1+10:00")]
        [InlineData("15:56:20.0+10:00")]
        [InlineData("15:56:20.10+10:00")]
        [InlineData("15:56:20.123+10:00")]
        [InlineData("15:56:20.1234567+10:00")]
        [InlineData("15:56:20-10:00")]
        [InlineData("15:56:20.1-10:00")]
        [InlineData("15:56:20.0-10:00")]
        [InlineData("15:56:20.10-10:00")]
        [InlineData("15:56:20.123-10:00")]
        [InlineData("15:56:20.1234567-10:00")]
        [InlineData("15:56:20Z")]
        [InlineData("15:56:20.1Z")]
        [InlineData("15:56:20.0Z")]
        [InlineData("15:56:20.10Z")]
        [InlineData("15:56:20.123Z")]
        [InlineData("15:56:20.1234567Z")]
        public void Validate_ValidTimeFormat_ShouldPassValidation(string validTimeValue)
        {
            string schema = """
             {
               "type": "string",
               "format": "time"
             }
             """;

            var jsonValidator = new JsonValidator(schema);

            string jsonInstance = $"\"{validTimeValue}\"";
            Assert.True(jsonValidator.Validate(jsonInstance).IsValid);
        }

        public static IEnumerable<object[]> JsonSchemaTestSuiteForDraft2020
        {
            get
            {
                IEnumerable<TestCase> testCases = TestSuiteReader.ReadTestCasesFromJsonSchemaTestSuite("draft2020-12", UnsupportedTestFiles, UnsupportedTestCases);
                return GenerateJsonSchemaTestDataParameters(testCases);
            }
        }

        public static IEnumerable<object[]> JsonSchemaTestCasesForFormatKeyword
        {
            get
            {
                IEnumerable<TestCase> testCases = TestSuiteReader.ReadTestCases(Path.Combine("TestData", "format.json"), Array.Empty<string>());
                return GenerateJsonSchemaTestDataParameters(testCases);
            }
        }

        public static IEnumerable<object[]> JsonSchemaTestCasesForCustomFormat
        {
            get
            {
                IEnumerable<TestCase> testCases = TestSuiteReader.ReadTestCases(Path.Combine("TestData", "custom_format.json"), Array.Empty<string>());
                return GenerateJsonSchemaTestDataParameters(testCases);
            }
        }

        private static IEnumerable<object[]> GenerateJsonSchemaTestDataParameters(IEnumerable<TestCase> testCases)
        {
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

        [Fact]
        public void Validate_CommonKeywordsValidateFail_ReportExpectedInfo()
        {
            string schema = File.ReadAllText(Path.Combine("TestData", "schema.json"));
            string instance = File.ReadAllText(Path.Combine("TestData", "instance.json"));

            var jsonValidator = new JsonValidator(schema);
            ValidationResult validationResult = jsonValidator.Validate(instance);

            Assert.False(validationResult.IsValid);
            Assert.Equal(ResultCode.InvalidTokenKind, validationResult.ResultCode);
            Assert.Equal("Expect type(s): 'Integer' but actual is 'String'", validationResult.ErrorMessage);
            Assert.Equal("type", validationResult.Keyword);
            Assert.Equal(ImmutableJsonPointer.Create("/propArray/4"), validationResult.InstanceLocation);
            Assert.Equal(ImmutableJsonPointer.Create("/properties/propArray/items/type"), validationResult.RelativeKeywordLocation);
            Assert.Equal(new Uri("http://main"), validationResult.SchemaResourceBaseUri);
            Assert.Equal(new Uri("http://main"), validationResult.SubSchemaRefFullUri);
        }

        [Theory]
        [InlineData("0.001", "0.001", true)]
        [InlineData("114201340.72", "0.001", true)]
        [InlineData("314201340.72", "0.001", true)]
        [InlineData("8355604201340.72", "0.001", true)]
        [InlineData("8355604201340.729", "0.001", true)]
        [InlineData("18355604201340.729", "0.001", true)]
        [InlineData("118355604201340.729", "0.001", true)]
        [InlineData("8355604201340.7201", "0.001", false)]

        [InlineData("-0.0001", "0.0001", true)]
        [InlineData("-114201340.7201", "0.0001", true)]
        [InlineData("-314201340.7201", "0.0001", true)]
        [InlineData("-8355604201340.7201", "0.0001", true)]
        [InlineData("-18355604201340.7201", "0.0001", true)]
        [InlineData("-118355604201340.7201", "0.0001", true)]
        [InlineData("-8355604201340.72001", "0.0001", false)]
        public void Validate_MultipleOf(string jsonInstance, string multipleOf, bool expectedValidationResult)
        {
            var jsonValidator = new JsonValidator($$"""{"multipleOf": {{multipleOf}} }""");
            bool actualValidationResult = jsonValidator.Validate(jsonInstance).IsValid;

            Assert.Equal(expectedValidationResult, actualValidationResult);
        }

        [Theory]
        [MemberData(nameof(TestDataForPropertyNameIgnoreCase))]
        public void Validate_PropertyNameIgnoreCase(string jsonSchema, string jsonInstance, bool expectedIsValid, string? expectedInstanceLocation, string? expectedKeywordLocation)
        {
            ValidationResult validationResult = new JsonValidator(jsonSchema, new JsonValidatorOptions{PropertyNameCaseInsensitive = true}).Validate(jsonInstance);

            Assert.Equal(expectedIsValid, validationResult.IsValid);
            Assert.Equal(expectedInstanceLocation, validationResult.InstanceLocation?.ToString());
            Assert.Equal(expectedKeywordLocation, validationResult.RelativeKeywordLocation?.ToString());
        }

        public static IEnumerable<object?[]> TestDataForPropertyNameIgnoreCase
        {
            get
            {
                yield return new object?[]
                {
                    """
                    {
                      "properties": {
                        "A": {"type": "string"}
                      }
                    }
                    """,
                    """ 
                    {
                      "a": "abc"
                    }
                    """,
                    true, null, null
                };

                yield return new object?[]
                {
                    """
                    {
                      "properties": {
                        "A": {"type": "string"}
                      }
                    }
                    """,
                    """ 
                    {
                      "a": 123
                    }
                    """,
                    false, "/a", "/properties/A/type"
                };

                yield return new object?[]
                {
                    """
                    {
                      "properties": {
                        "A": {
                               "properties": { 
                                 "B": {"type": "string"} 
                        }
                      }
                      }
                    }
                    """,
                    """ 
                    {
                      "a": {
                        "b": 123
                      }
                    }
                    """,
                    false, "/a/b", "/properties/A/properties/B/type"
                };

                yield return new object?[]
                {
                    """
                    {
                      "properties": {
                        "A": {
                               "dependentSchemas": { 
                                 "B": { "maxProperties": 1 } 
                        }
                      }
                      }
                    }
                    """,
                    """ 
                    {
                      "a": {
                        "c": 0,
                        "d": 1
                      }
                    }
                    """,
                    true, null, null
                };

                yield return new object?[]
                {
                    """
                    {
                      "properties": {
                        "A": {
                               "dependentSchemas": { 
                                 "B": { "maxProperties": 1 } 
                        }
                      }
                      }
                    }
                    """,
                    """ 
                    {
                      "a": {
                        "b": 0,
                        "c": 1
                      }
                    }
                    """,
                    false, "/a", "/properties/A/dependentSchemas/B/maxProperties"
                };

                yield return new object?[]
                {
                    """
                    {
                      "properties": {
                        "A": {
                               "required": [ "B" ]
                        }
                      }
                    }
                    """,
                    """ 
                    {
                      "a": {
                        "b": 0
                      }
                    }
                    """,
                    true, null, null
                };

                yield return new object?[]
                {
                    """
                    {
                      "properties": {
                        "A": {
                               "required": [ "B" ]
                        }
                      }
                    }
                    """,
                    """ 
                    {
                      "a": {
                        "c": 0
                      }
                    }
                    """,
                    false, "/a", "/properties/A/required"
                };
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
            public static IEnumerable<TestCase> ReadTestCasesFromJsonSchemaTestSuite(string draftVersion, string[] unsupportedKeywords, string[] unsupportedTestCases)
            {
                string[] pathFiles = Directory.GetFiles(Path.Combine("JSON-Schema-Test-Suite", "tests", draftVersion));

                IEnumerable<TestCase> result = Enumerable.Empty<TestCase>();

                foreach (string pathFile in pathFiles)
                {
                    if (IsFileForUnsupportedKeyword(pathFile, unsupportedKeywords))
                    {
                        continue;
                    }

                    result = result.Concat(ReadTestCases(pathFile, unsupportedTestCases));
                }

                return result;
            }

            public static IEnumerable<TestCase> ReadTestCases(string pathFile, string[] unsupportedTestCases)
            {
                using (FileStream fs = File.OpenRead(pathFile))
                {
                    TestCase[] testCases = JsonSerializer.Deserialize<TestCase[]>(fs, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                    foreach (TestCase testCase in testCases)
                    {
                        if (!IsUnsupportedTestCase(testCase, unsupportedTestCases))
                        {
                            yield return testCase;
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