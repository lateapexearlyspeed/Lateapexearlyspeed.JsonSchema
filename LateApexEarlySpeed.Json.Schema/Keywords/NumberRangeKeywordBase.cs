using System.Diagnostics;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal abstract class NumberRangeKeywordBase : KeywordBase
{
    private readonly IBenchmarkChecker _benchmarkChecker;

    public object BenchmarkValue => _benchmarkChecker.BenchmarkValue;

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

        public abstract object BenchmarkValue { get; }

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
        protected readonly decimal Benchmark;

        protected DecimalBenchmarkCheckerBase(decimal benchmark)
        {
            Benchmark = benchmark;
        }

        public object BenchmarkValue => Benchmark;

        public bool IsInRange(JsonInstanceElement instance)
        {
            return instance.TryGetDecimal(out decimal value) 
                ? IsInRange(value) 
                : IsInRange(instance.GetDouble());
        }

        protected abstract bool IsInRange(decimal instanceValue);
        protected abstract bool IsInRange(double instanceValue);

        public string GetErrorMessage(JsonInstanceElement instance)
        {
            return instance.TryGetDecimal(out decimal value) 
                ? GetErrorMessage(value) 
                : GetErrorMessage(instance.GetDouble());
        }

        protected abstract string GetErrorMessage(object instanceValue);
    }

    protected interface IBenchmarkChecker
    {
        /// <summary>
        /// This represents numeric benchmark value, the possible numeric types are: long, ulong, double, decimal
        /// </summary>
        object BenchmarkValue { get; }

        bool IsInRange(JsonInstanceElement instance);

        string GetErrorMessage(JsonInstanceElement instance);
    }
}
