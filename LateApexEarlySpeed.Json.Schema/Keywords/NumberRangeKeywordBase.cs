using System.Diagnostics;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal abstract class NumberRangeKeywordBase : KeywordBase
{
    private readonly BenchmarkChecker _benchmarkChecker;

    protected NumberRangeKeywordBase(BenchmarkChecker benchmarkChecker)
    {
        _benchmarkChecker = benchmarkChecker;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number)
        {
            return ValidationResult.ValidResult;
        }

        instance.GetNumericValue(out double? instanceDoubleValue, out long? instanceSignedLongValue, out ulong? instanceUnsignedLongValue);

        if (instanceDoubleValue.HasValue)
        {
            return _benchmarkChecker.IsInRange(instanceDoubleValue.Value)
                ? ValidationResult.ValidResult
                : ValidationResult.CreateFailedResult(ResultCode.NumberOutOfRange, _benchmarkChecker.GetErrorMessage(instanceDoubleValue.Value), options.ValidationPathStack, Name, instance.Location);
        }

        if (instanceSignedLongValue.HasValue)
        {
            return _benchmarkChecker.IsInRange(instanceSignedLongValue.Value)
                ? ValidationResult.ValidResult
                : ValidationResult.CreateFailedResult(ResultCode.NumberOutOfRange, _benchmarkChecker.GetErrorMessage(instanceSignedLongValue.Value), options.ValidationPathStack, Name, instance.Location);
        }

        Debug.Assert(instanceUnsignedLongValue.HasValue);
        return _benchmarkChecker.IsInRange(instanceUnsignedLongValue.Value)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.NumberOutOfRange, _benchmarkChecker.GetErrorMessage(instanceUnsignedLongValue.Value), options.ValidationPathStack, Name, instance.Location);
    }

    protected abstract class BenchmarkChecker
    {
        public abstract bool IsInRange(double instanceValue);
        public abstract bool IsInRange(long instanceValue);
        public abstract bool IsInRange(ulong instanceValue);

        public abstract string GetErrorMessage(object instanceValue);
    }
}