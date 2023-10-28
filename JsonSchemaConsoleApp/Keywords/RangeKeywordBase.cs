using System.Text.Json;

namespace JsonSchemaConsoleApp.Keywords;

internal abstract class RangeKeywordBase : KeywordBase
{
    protected readonly double BenchmarkValue;

    protected RangeKeywordBase(double benchmarkValue)
    {
        BenchmarkValue = benchmarkValue;
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number)
        {
            return ValidationResult.ValidResult;
        }

        return IsInRange(instance.GetDouble())
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.NumberOutOfRange, options.ValidationPathStack);
    }

    protected abstract bool IsInRange(double instanceValue);
}