using System.Diagnostics;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal abstract class NumberRangeKeywordBase : KeywordBase
{
    private readonly IBenchmarkChecker _benchmarkChecker;

    protected NumberRangeKeywordBase(IBenchmarkChecker benchmarkChecker)
    {
        _benchmarkChecker = benchmarkChecker;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number)
        {
            return ValidationResult.ValidResult;
        }

        return _benchmarkChecker.IsInRange(instance)
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.NumberOutOfRange, _benchmarkChecker.GetErrorMessage(instance), options.ValidationPathStack, Name, instance.Location);
    }

    protected abstract class NonDecimalBenchmarkCheckerBase : IBenchmarkChecker
    {
        public bool IsInRange(JsonInstanceElement instance)
        {
            instance.GetNumericValue(out double? instanceDoubleValue, out long? instanceSignedLongValue, out ulong? instanceUnsignedLongValue);

            if (instanceSignedLongValue.HasValue)
            {
                return IsInRange(instanceSignedLongValue.Value);
            }

            if (instanceUnsignedLongValue.HasValue)
            {
                return IsInRange(instanceUnsignedLongValue.Value);
            }

            Debug.Assert(instanceDoubleValue.HasValue);
            return IsInRange(instanceDoubleValue.Value);
        }

        protected abstract bool IsInRange(double instanceValue);
        protected abstract bool IsInRange(long instanceValue);
        protected abstract bool IsInRange(ulong instanceValue);

        public string GetErrorMessage(JsonInstanceElement instance)
        {
            instance.GetNumericValue(out double? instanceDoubleValue, out long? instanceSignedLongValue, out ulong? instanceUnsignedLongValue);

            if (instanceSignedLongValue.HasValue)
            {
                return GetErrorMessage(instanceSignedLongValue.Value);
            }

            if (instanceUnsignedLongValue.HasValue)
            {
                return GetErrorMessage(instanceUnsignedLongValue.Value);
            }

            Debug.Assert(instanceDoubleValue.HasValue);
            return GetErrorMessage(instanceDoubleValue.Value);
        }

        protected abstract string GetErrorMessage(object instanceValue);
    }

    protected abstract class DecimalBenchmarkCheckerBase : IBenchmarkChecker
    {
        protected readonly decimal BenchmarkValue;

        protected DecimalBenchmarkCheckerBase(decimal benchmark)
        {
            BenchmarkValue = benchmark;
        }

        public bool IsInRange(JsonInstanceElement instance)
        {
            if (!instance.TryGetDecimal(out decimal value))
            {
                return false;
            }

            return IsInRange(value);
        }

        protected abstract bool IsInRange(decimal instanceValue);

        public string GetErrorMessage(JsonInstanceElement instance)
        {
            if (instance.TryGetDecimal(out decimal value))
            {
                return GetErrorMessage(value);
            }

            object numericValue;
            instance.GetNumericValue(out double? doubleValue, out long? longValue, out ulong? ulongValue);
            if (longValue.HasValue)
            {
                numericValue = longValue.Value;
            }
            else if (ulongValue.HasValue)
            {
                numericValue = ulongValue.Value;
            }
            else
            {
                Debug.Assert(doubleValue.HasValue);
                numericValue = doubleValue.Value;
            }

            return $"Instance value: '{numericValue}' cannot be converted to decimal.";
        }

        protected abstract string GetErrorMessage(object instanceValue);
    }

    protected interface IBenchmarkChecker
    {
        bool IsInRange(JsonInstanceElement instance);

        string GetErrorMessage(JsonInstanceElement instance);
    }
}
