using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal abstract class NumberRangeKeywordBase : KeywordBase
{
    public double BenchmarkValue { get; init; }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number)
        {
            return ValidationResult.ValidResult;
        }

        double instanceValue = instance.GetDouble();
        return IsInRange(instanceValue)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.NumberOutOfRange, GetErrorMessage(instanceValue), options.ValidationPathStack, Name, instance.Location);
    }

    protected abstract bool IsInRange(double instanceValue);

    protected abstract string GetErrorMessage(double instanceValue);
}