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

            Assert.Equal(expectedValidationResult, jsonValidator.Validate(instance, new JsonSchemaOptions{ValidateFormat = false}).IsValid);
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