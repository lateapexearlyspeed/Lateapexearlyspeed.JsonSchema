using System.Diagnostics;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using ValidationResult = LateApexEarlySpeed.Json.Schema.Common.ValidationResult;

namespace Json.Schema.Libraries.Benchmark;

internal class BigDataBenchmarkTests
{
    private readonly string _schema = File.ReadAllText(Path.Combine("TestData", "schema-json-everything-issues-766.json"));
    private readonly string _instance = File.ReadAllText(Path.Combine("TestData", "instance-json-everything-issues-766.json"));

    public void ValidateByLateApexEarlySpeed()
    {
        Stopwatch sw = Stopwatch.StartNew();

        ValidationResult validationResult = new JsonValidator(_schema).Validate(_instance, new JsonSchemaOptions{OutputFormat = LateApexEarlySpeed.Json.Schema.Common.OutputFormat.List});

        Console.WriteLine($"{validationResult.IsValid}, errors: {validationResult.ValidationErrors.Count()}, time: {sw.Elapsed}");
    }

    public void ValidateByJsonSchemaDotNet()
    {
        Stopwatch sw = Stopwatch.StartNew();

        JsonSchema jsonSchema = JsonSchema.FromText(_schema);

        using (JsonDocument data = JsonDocument.Parse(_instance))
        {
            EvaluationResults evaluationResults = jsonSchema.Evaluate(data, new EvaluationOptions{OutputFormat = OutputFormat.List});

            Console.WriteLine($"{evaluationResults.IsValid}, errors: {evaluationResults.Details.Count}, time: {sw.Elapsed}");
        }
    }
}