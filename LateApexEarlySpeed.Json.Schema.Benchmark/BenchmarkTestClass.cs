using BenchmarkDotNet.Attributes;
using System.Text.Json;
using Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Benchmark;

[MemoryDiagnoser]
public class BenchmarkTestClass
{
    private JsonSchema _jsonSchemaDotnet;
    private JsonValidator _jsonValidator;
    private string _instanceText;
    private string _schemaText;

    [GlobalSetup]
    public void Setup()
    {
        _schemaText = File.ReadAllText("schema.json");

        _jsonSchemaDotnet = JsonSchema.FromText(_schemaText);
        _jsonValidator = new JsonValidator(_schemaText);

        _instanceText = File.ReadAllText("instance.json");

        EvaluationResults evaluationResults = ValidateByJsonSchemaDotNet();
        if (!evaluationResults.IsValid)
        {
            throw new Exception("Benchmark test should use a happy path case.");
        }

        ValidationResult validationResult = ValidateByMyJsonSchema();
        if (!validationResult.IsValid)
        {
            throw new Exception("Benchmark test should use a happy path case.");
        }
    }

    [Benchmark]
    public EvaluationResults ValidateByJsonSchemaDotNet()
    {
        using (var instance = JsonDocument.Parse(_instanceText))
        {
            return JsonSchema.FromText(_schemaText).Evaluate(instance);
        }
    }

    [Benchmark]
    public ValidationResult ValidateByMyJsonSchema()
    {
        return new JsonValidator(_schemaText).Validate(_instanceText);
    }

    [Benchmark]
    public EvaluationResults ValidateByReuseJsonSchemaDotNet()
    {
        using (var instance = JsonDocument.Parse(_instanceText))
        {
            return _jsonSchemaDotnet.Evaluate(instance);
        }
    }

    [Benchmark]
    public ValidationResult ValidateByReuseMyJsonSchema()
    {
        return _jsonValidator.Validate(_instanceText);
    }
}