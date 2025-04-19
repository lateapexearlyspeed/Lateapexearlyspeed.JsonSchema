using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.Common;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using JsonValidator = LateApexEarlySpeed.Json.Schema.JsonValidator;

namespace Json.Schema.Libraries.Benchmark;

public class JsonSchemaValidationRunner
{
    private static readonly string[] UnsupportedTestFiles = new[] { "unevaluatedItems", "unevaluatedProperties", "vocabulary" };

    private static readonly string[] UnsupportedTestCases = Array.Empty<string>();


    private readonly TestCase[] _testCases;

    public JsonSchemaValidationRunner(IJsonSchemaValidation[] jsonSchemaValidations)
    {
        TestCase[] testCases = TestSuiteReader.ReadTestCasesFromJsonSchemaTestSuite("draft2020-12", UnsupportedTestFiles, UnsupportedTestCases);

        var finalTestCases = new List<TestCase>();

        foreach (TestCase testCase in testCases)
        {
            var passedTests = new List<Test>(testCase.Tests.Length);

            foreach (Test test in testCase.Tests)
            {
                bool allValidationsCanPass = true;

                foreach (IJsonSchemaValidation jsonSchemaValidation in jsonSchemaValidations)
                {
                    try
                    {
                        bool actualValidationResult = jsonSchemaValidation.Validate(testCase.JsonSchema, test.Instance);

                        if (actualValidationResult != test.ValidationResult)
                        {
                            allValidationsCanPass = false;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        allValidationsCanPass = false;
                        break;
                    }
                }

                if (allValidationsCanPass)
                {
                    passedTests.Add(test);
                }
            }

            if (passedTests.Count != 0)
            {
                testCase.Tests = passedTests.ToArray();

                foreach (IJsonSchemaValidation jsonSchemaValidation in jsonSchemaValidations)
                {
                    switch (jsonSchemaValidation.LibraryKinds)
                    {
                        case JsonSchemaLibraryKinds.LateApexEarlySpeed:
                            testCase.LateApexEarlySpeedValidator = new JsonValidator(testCase.JsonSchema);
                            break;
                        case JsonSchemaLibraryKinds.JsonSchemaDotNet:
                            testCase.JsonSchemaDotNetValidator = JsonSchema.FromText(testCase.JsonSchema);
                            break;
                        case JsonSchemaLibraryKinds.NJsonSchema:
                            testCase.NJsonSchemaValidator = NJsonSchema.JsonSchema.FromJsonAsync(testCase.JsonSchema).Result;
                            break;
                        case JsonSchemaLibraryKinds.Newtonsoft:
                            testCase.NewtonsoftValidator = JSchema.Parse(testCase.JsonSchema);
                            break;
                        default:
                            throw new NotSupportedException($"Not support library kind: {jsonSchemaValidation.LibraryKinds}");
                    }
                }

                finalTestCases.Add(testCase);
            }
        }

        Console.WriteLine($"Total tests count: {finalTestCases.Sum(tc => tc.Tests.Length)}");

        _testCases = finalTestCases.ToArray();
    }

    public bool CreateNewSchema_LateApexEarlySpeed()
    {
        bool result = false;

        var jsonSchemaOptions = new JsonSchemaOptions { ValidateFormat = false };

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                ValidationResult validationResult = new JsonValidator(testCase.JsonSchema).Validate(test.Instance, jsonSchemaOptions);

                // Just for make sure code above is invoked
                if (validationResult.IsValid)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public bool ReuseSchema_LateApexEarlySpeed(TestValidationResult testValidationResult)
    {
        bool result = false;

        var jsonSchemaOptions = new JsonSchemaOptions { ValidateFormat = false };

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                if ((test.ToTestValidationResult & testValidationResult) == 0)
                {
                    continue;
                }

                Debug.Assert(testCase.LateApexEarlySpeedValidator is not null);
                ValidationResult validationResult = testCase.LateApexEarlySpeedValidator.Validate(test.Instance, jsonSchemaOptions);

                // Just for make sure code above is invoked
                if (validationResult.IsValid)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public bool CreateNewSchema_JsonSchemaDotNet()
    {
        bool result = false;

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                JsonSchema schema = JsonSchema.FromText(testCase.JsonSchema);

                // Just for make sure Evaluate() is invoked
                if (schema.Evaluate(JsonNode.Parse(test.Instance)).IsValid)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public bool ReuseSchema_JsonSchemaDotNet(TestValidationResult testValidationResult)
    {
        bool result = false;

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                if ((test.ToTestValidationResult & testValidationResult) == 0)
                {
                    continue;
                }

                Debug.Assert(testCase.JsonSchemaDotNetValidator is not null);

                // Just for make sure Evaluate() is invoked
                if (testCase.JsonSchemaDotNetValidator.Evaluate(JsonNode.Parse(test.Instance)).IsValid)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public bool CreateNewSchema_NJsonSchema()
    {
        bool result = false;

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                NJsonSchema.JsonSchema schema = NJsonSchema.JsonSchema.FromJsonAsync(testCase.JsonSchema).Result;

                ICollection<NJsonSchema.Validation.ValidationError> validationErrors = schema.Validate(test.Instance);

                // Just for make sure code above is invoked
                if (validationErrors.Count == 0)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public bool ReuseSchema_NJsonSchema(TestValidationResult testValidationResult)
    {
        bool result = false;

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                if ((test.ToTestValidationResult & testValidationResult) == 0)
                {
                    continue;
                }

                Debug.Assert(testCase.NJsonSchemaValidator is not null);
                ICollection<NJsonSchema.Validation.ValidationError> validationErrors = testCase.NJsonSchemaValidator.Validate(test.Instance);

                // Just for make sure code above is invoked
                if (validationErrors.Count == 0)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public bool CreateNewSchema_Newtonsoft()
    {
        bool result = false;

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                JSchema jSchema = JSchema.Parse(testCase.JsonSchema);

                bool isValid = JToken.Parse(test.Instance).IsValid(jSchema);

                // Just for make sure code above is invoked
                if (isValid)
                {
                    result = true;
                }
            }
        }

        return result;
    }

    public bool ReuseSchema_Newtonsoft(TestValidationResult testValidationResult)
    {
        bool result = false;

        foreach (TestCase testCase in _testCases)
        {
            foreach (Test test in testCase.Tests)
            {
                if ((test.ToTestValidationResult & testValidationResult) == 0)
                {
                    continue;
                }

                Debug.Assert(testCase.NewtonsoftValidator is not null);
                bool isValid = JToken.Parse(test.Instance).IsValid(testCase.NewtonsoftValidator);

                // Just for make sure code above is invoked
                if (isValid)
                {
                    result = true;
                }
            }
        }

        return result;
    }
}