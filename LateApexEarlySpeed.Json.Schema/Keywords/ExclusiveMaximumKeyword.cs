using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("exclusiveMaximum")]
[JsonConverter(typeof(ExclusiveMaximumKeywordJsonConverter))]
internal class ExclusiveMaximumKeyword : NumberRangeKeywordBase
{
    public ExclusiveMaximumKeyword(double max) : base(new DoubleTypeBenchmarkCheckerBase(max))
    {
    }

    public ExclusiveMaximumKeyword(long max) : base(new LongTypeBenchmarkCheckerBase(max))
    {
    }

    public ExclusiveMaximumKeyword(ulong max) : base(new UnsignedLongTypeBenchmarkCheckerBase(max))
    {
    }

    public ExclusiveMaximumKeyword(decimal max) : base(new DecimalBenchmarkChecker(max))
    {
    }

    public static string ErrorMessage(object instanceValue, object maximum)
    {
        return $"Instance '{instanceValue}' is equal to or greater than '{maximum}'";
    }

    private class DoubleTypeBenchmarkCheckerBase : NonDecimalBenchmarkCheckerBase
    {
        private readonly double _benchmark;

        public DoubleTypeBenchmarkCheckerBase(double benchmark)
        {
            _benchmark = benchmark;
        }

        public override object BenchmarkValue => _benchmark;

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue < _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            return instanceValue < _benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            return instanceValue < _benchmark;
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

        public override object BenchmarkValue => _benchmark;

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue < _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            return instanceValue < _benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            if (_benchmark < 0)
            {
                return false;
            }
            return instanceValue < (ulong)_benchmark;
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

        public override object BenchmarkValue => _benchmark;

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue < _benchmark;
        }

        protected override bool IsInRange(long instanceValue)
        {
            if (_benchmark > long.MaxValue)
            {
                return true;
            }

            return instanceValue < (long)_benchmark;
        }

        protected override bool IsInRange(ulong instanceValue)
        {
            return instanceValue < _benchmark;
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
            return instanceValue < Benchmark;
        }

        protected override bool IsInRange(double instanceValue)
        {
            return instanceValue < (double)Benchmark;
        }

        protected override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, Benchmark);
        }
    }
}