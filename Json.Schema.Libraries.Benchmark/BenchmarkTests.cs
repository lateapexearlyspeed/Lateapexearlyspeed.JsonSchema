using BenchmarkDotNet.Attributes;

namespace Json.Schema.Libraries.Benchmark;

[MemoryDiagnoser]
public class BenchmarkTests
{
    private static readonly IJsonSchemaValidation[] JsonSchemaValidations = new IJsonSchemaValidation[]
    {
        new LateApexEarlySpeedValidation(),
        new JsonSchemaDotNetValidation(),
        new NJsonSchemaValidation()
    };

    private JsonSchemaValidationRunner _jsonSchemaValidationRunner = null!;

    [GlobalSetup]
    public void PrepareTestCases()
    {
        _jsonSchemaValidationRunner = new JsonSchemaValidationRunner(JsonSchemaValidations);
    }

    [Benchmark]
    public bool CreateNewSchema_LateApexEarlySpeed()
    {
        return _jsonSchemaValidationRunner.CreateNewSchema_LateApexEarlySpeed();
    }

    [Benchmark]
    [Arguments(TestValidationResult.Positive)]
    [Arguments(TestValidationResult.Negative)]
    [Arguments(TestValidationResult.All)]
    public bool ReuseSchema_LateApexEarlySpeed(TestValidationResult testValidationResult)
    {
        return _jsonSchemaValidationRunner.ReuseSchema_LateApexEarlySpeed(testValidationResult);
    }

    [Benchmark]
    public bool CreateNewSchema_JsonSchemaDotNet()
    {
        return _jsonSchemaValidationRunner.CreateNewSchema_JsonSchemaDotNet();
    }

    [Benchmark]
    [Arguments(TestValidationResult.Positive)]
    [Arguments(TestValidationResult.Negative)]
    [Arguments(TestValidationResult.All)]
    public bool ReuseSchema_JsonSchemaDotNet(TestValidationResult testValidationResult)
    {
        return _jsonSchemaValidationRunner.ReuseSchema_JsonSchemaDotNet(testValidationResult);
    }

    [Benchmark]
    public bool CreateNewSchema_NJsonSchema()
    {
        return _jsonSchemaValidationRunner.CreateNewSchema_NJsonSchema();
    }

    [Benchmark]
    [Arguments(TestValidationResult.Positive)]
    [Arguments(TestValidationResult.Negative)]
    [Arguments(TestValidationResult.All)]
    public bool ReuseSchema_NJsonSchema(TestValidationResult testValidationResult)
    {
        return _jsonSchemaValidationRunner.ReuseSchema_NJsonSchema(testValidationResult);
    }
}