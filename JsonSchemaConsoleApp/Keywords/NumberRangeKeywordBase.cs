using JsonSchemaConsoleApp.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonSchemaConsoleApp.Keywords;

internal abstract class NumberRangeKeywordBase : KeywordBase
{
    public double BenchmarkValue { get; init; }

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