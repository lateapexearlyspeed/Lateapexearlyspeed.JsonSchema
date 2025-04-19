using System.Diagnostics;

namespace Json.Schema.Libraries.Benchmark;

public class StopwatchTests
{
    private static readonly IJsonSchemaValidation[] JsonSchemaValidations = new IJsonSchemaValidation[]
    {
        new LateApexEarlySpeedValidation(),
        new JsonSchemaDotNetValidation(),
        new NJsonSchemaValidation(),
        new NewtonsoftValidation()
    };

    private readonly JsonSchemaValidationRunner _jsonSchemaValidationRunner;

    public StopwatchTests()
    {
        _jsonSchemaValidationRunner = new JsonSchemaValidationRunner(JsonSchemaValidations);
    }

    public void Run()
    {
        CreateNewSchema_LateApexEarlySpeed();
        ReuseSchema_LateApexEarlySpeed(TestValidationResult.Positive);
        ReuseSchema_LateApexEarlySpeed(TestValidationResult.Negative);
        ReuseSchema_LateApexEarlySpeed(TestValidationResult.All);

        CreateNewSchema_JsonSchemaDotNet();
        ReuseSchema_JsonSchemaDotNet(TestValidationResult.Positive);
        ReuseSchema_JsonSchemaDotNet(TestValidationResult.Negative);
        ReuseSchema_JsonSchemaDotNet(TestValidationResult.All);

        CreateNewSchema_NJsonSchema();
        ReuseSchema_NJsonSchema(TestValidationResult.Positive);
        ReuseSchema_NJsonSchema(TestValidationResult.Negative);
        ReuseSchema_NJsonSchema(TestValidationResult.All);

        CreateNewSchema_Newtonsoft();
        ReuseSchema_Newtonsoft(TestValidationResult.Positive);
        ReuseSchema_Newtonsoft(TestValidationResult.Negative);
        ReuseSchema_Newtonsoft(TestValidationResult.All);
    }

    public void CreateNewSchema_LateApexEarlySpeed()
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.CreateNewSchema_LateApexEarlySpeed());

        Console.WriteLine($"{nameof(CreateNewSchema_LateApexEarlySpeed)} takes {elapsedTime}");
    }

    public void ReuseSchema_LateApexEarlySpeed(TestValidationResult testValidationResult)
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.ReuseSchema_LateApexEarlySpeed(testValidationResult));

        Console.WriteLine($"{nameof(ReuseSchema_LateApexEarlySpeed)} with argument: {testValidationResult} takes {elapsedTime}");
    }

    public void CreateNewSchema_JsonSchemaDotNet()
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.CreateNewSchema_JsonSchemaDotNet());

        Console.WriteLine($"{nameof(CreateNewSchema_JsonSchemaDotNet)} takes {elapsedTime}");
    }

    public void ReuseSchema_JsonSchemaDotNet(TestValidationResult testValidationResult)
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.ReuseSchema_JsonSchemaDotNet(testValidationResult));

        Console.WriteLine($"{nameof(ReuseSchema_JsonSchemaDotNet)} with argument: {testValidationResult} takes {elapsedTime}");
    }

    public void CreateNewSchema_NJsonSchema()
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.CreateNewSchema_NJsonSchema());

        Console.WriteLine($"{nameof(CreateNewSchema_NJsonSchema)} takes {elapsedTime}");
    }

    public void ReuseSchema_NJsonSchema(TestValidationResult testValidationResult)
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.ReuseSchema_NJsonSchema(testValidationResult));

        Console.WriteLine($"{nameof(ReuseSchema_NJsonSchema)} with argument: {testValidationResult} takes {elapsedTime}");
    }

    public void CreateNewSchema_Newtonsoft()
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.CreateNewSchema_Newtonsoft());

        Console.WriteLine($"{nameof(CreateNewSchema_Newtonsoft)} takes {elapsedTime}");
    }

    public void ReuseSchema_Newtonsoft(TestValidationResult testValidationResult)
    {
        TimeSpan elapsedTime = MeasureElapsedTime(() => _jsonSchemaValidationRunner.ReuseSchema_Newtonsoft(testValidationResult));

        Console.WriteLine($"{nameof(ReuseSchema_Newtonsoft)} with argument: {testValidationResult} takes {elapsedTime}");
    }

    private static TimeSpan MeasureElapsedTime(Action measuredAction)
    {
        Stopwatch sw = Stopwatch.StartNew();

        measuredAction();

        return sw.Elapsed;
    }
}