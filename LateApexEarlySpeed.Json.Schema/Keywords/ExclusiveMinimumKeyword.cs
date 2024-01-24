using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("exclusiveMinimum")]
[JsonConverter(typeof(ExclusiveMinimumKeywordJsonConverter))]
internal class ExclusiveMinimumKeyword : NumberRangeKeywordBase
{
    public ExclusiveMinimumKeyword(double min) : base(new DoubleTypeBenchmarkCheckerBase(min))
    {
    }

    public ExclusiveMinimumKeyword(long min) : base(new LongTypeBenchmarkCheckerBase(min))
    {
    }

    public ExclusiveMinimumKeyword(ulong min) : base(new UnsignedLongTypeBenchmarkCheckerBase(min))
    {
    }

    public ExclusiveMinimumKeyword(decimal min) : base(new DecimalBenchmarkChecker(min))
    {
    }

    [Obfuscation]
    public static string ErrorMessage(object instanceValue, object minimum)
    {
        return $"Instance '{instanceValue}' is equal to or less than '{minimum}'";
    }

    private class DoubleTypeBenchmarkCheckerBase : NonDecimalBenchmarkCheckerBase
    {
        private readonly double _benchmark;

        public DoubleTypeBenchmarkCheckerBase(double benchmark)
        {
            _benchmark = benchmark;
        }

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue > _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            return instanceValue > _benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            return instanceValue > _benchmark;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }

    private class LongTypeBenchmarkCheckerBase : NonDecimalBenchmarkCheckerBase
    {
        private readonly long _benchmark;

        public LongTypeBenchmarkCheckerBase(long benchmark)
        {
            _benchmark = benchmark;
        }

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue > _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            return instanceValue > _benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            if (_benchmark < 0)
            {
                return true;
            }
            return instanceValue > (ulong)_benchmark;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }

    private class UnsignedLongTypeBenchmarkCheckerBase : NonDecimalBenchmarkCheckerBase
    {
        private readonly ulong _benchmark;

        public UnsignedLongTypeBenchmarkCheckerBase(ulong benchmark)
        {
            _benchmark = benchmark;
        }

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue > _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            if (_benchmark > long.MaxValue)
            {
                return false;
            }

            return instanceValue > (long)_benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            return instanceValue > _benchmark;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }

    private class DecimalBenchmarkChecker : DecimalBenchmarkCheckerBase
    {
        public DecimalBenchmarkChecker(decimal benchmark) : base(benchmark)
        {
        }

        protected override bool IsInRange(decimal instanceValue)
        {
            return instanceValue > BenchmarkValue;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, BenchmarkValue);
        }
    }
}