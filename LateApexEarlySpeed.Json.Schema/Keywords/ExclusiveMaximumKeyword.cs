using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("exclusiveMaximum")]
[JsonConverter(typeof(ExclusiveMaximumKeywordJsonConverter))]
internal class ExclusiveMaximumKeyword : NumberRangeKeywordBase
{
    public ExclusiveMaximumKeyword(double max) : base(new DoubleTypeBenchmarkChecker(max))
    {
    }

    public ExclusiveMaximumKeyword(long max) : base(new LongTypeBenchmarkChecker(max))
    {
    }

    public ExclusiveMaximumKeyword(ulong max) : base(new UnsignedLongTypeBenchmarkChecker(max))
    {
    }

    [Obfuscation]
    public static string ErrorMessage(object instanceValue, object maximum)
    {
        return $"Instance '{instanceValue}' is equal to or greater than '{maximum}'";
    }

    private class DoubleTypeBenchmarkChecker : BenchmarkChecker
    {
        private readonly double _benchmark;

        public DoubleTypeBenchmarkChecker(double benchmark)
        {
            _benchmark = benchmark;
        }

        public override bool IsInRange(double instanceValue)
        {
            return instanceValue < _benchmark;
        }

        public override bool IsInRange(long instanceValue)
        {
            return instanceValue < _benchmark;
        }

        public override bool IsInRange(ulong instanceValue)
        {
            return instanceValue < _benchmark;
        }

        public override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }

    private class LongTypeBenchmarkChecker : BenchmarkChecker
    {
        private readonly long _benchmark;

        public LongTypeBenchmarkChecker(long benchmark)
        {
            _benchmark = benchmark;
        }

        public override bool IsInRange(double instanceValue)
        {
            return instanceValue < _benchmark;
        }

        public override bool IsInRange(long instanceValue)
        {
            return instanceValue < _benchmark;
        }

        public override bool IsInRange(ulong instanceValue)
        {
            if (_benchmark < 0)
            {
                return false;
            }
            return instanceValue < (ulong)_benchmark;
        }

        public override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }

    private class UnsignedLongTypeBenchmarkChecker : BenchmarkChecker
    {
        private readonly ulong _benchmark;

        public UnsignedLongTypeBenchmarkChecker(ulong benchmark)
        {
            _benchmark = benchmark;
        }

        public override bool IsInRange(double instanceValue)
        {
            return instanceValue < _benchmark;
        }

        public override bool IsInRange(long instanceValue)
        {
            if (_benchmark > long.MaxValue)
            {
                return true;
            }

            return instanceValue < (long)_benchmark;
        }

        public override bool IsInRange(ulong instanceValue)
        {
            return instanceValue < _benchmark;
        }

        public override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }
}