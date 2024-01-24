using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("maximum")]
[JsonConverter(typeof(MaximumKeywordJsonConverter))]
internal class MaximumKeyword : NumberRangeKeywordBase
{
    public MaximumKeyword(double max) : base(new DoubleTypeBenchmarkChecker(max))
    {
    }

    public MaximumKeyword(long max) : base(new LongTypeBenchmarkChecker(max))
    {
    }

    public MaximumKeyword(ulong max) : base(new UnsignedLongTypeBenchmarkChecker(max))
    {
    }

    public MaximumKeyword(decimal max) : base(new DecimalBenchmarkChecker(max))
    {
    }

    [Obfuscation]
    public static string ErrorMessage(object instanceValue, object maximum)
    {
        return $"Instance '{instanceValue}' is greater than '{maximum}'";
    }

    private class DoubleTypeBenchmarkChecker : NonDecimalBenchmarkCheckerBase
    {
        private readonly double _benchmark;

        public DoubleTypeBenchmarkChecker(double benchmark)
        {
            _benchmark = benchmark;
        }

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue <= _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            return instanceValue <= _benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            return instanceValue <= _benchmark;
        }

        protected override string GetErrorMessage(object instance)
        {
            return ErrorMessage(instance, _benchmark);
        }
    }

    private class LongTypeBenchmarkChecker : NonDecimalBenchmarkCheckerBase
    {
        private readonly long _benchmark;

        public LongTypeBenchmarkChecker(long benchmark)
        {
            _benchmark = benchmark;
        }

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue <= _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            return instanceValue <= _benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            if (_benchmark < 0)
            {
                return false;
            }
            return instanceValue <= (ulong)_benchmark;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }

    private class UnsignedLongTypeBenchmarkChecker : NonDecimalBenchmarkCheckerBase
    {
        private readonly ulong _benchmark;

        public UnsignedLongTypeBenchmarkChecker(ulong benchmark)
        {
            _benchmark = benchmark;
        }

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue <= _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            if (_benchmark > long.MaxValue)
            {
                return true;
            }

            return instanceValue <= (long)_benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            return instanceValue <= _benchmark;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }

    private class DecimalBenchmarkChecker : DecimalBenchmarkCheckerBase
    {
        public DecimalBenchmarkChecker(decimal max) : base(max)
        {
        }

        protected override bool IsInRange(decimal instanceValue)
        {
            return instanceValue <= BenchmarkValue;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, BenchmarkValue);
        }
    }
}
