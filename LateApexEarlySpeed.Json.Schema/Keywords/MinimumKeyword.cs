using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("minimum")]
[JsonConverter(typeof(MinimumKeywordJsonConverter))]
internal class MinimumKeyword : NumberRangeKeywordBase
{
    public MinimumKeyword(double min) : base(new DoubleTypeBenchmarkChecker(min))
    {
    }

    public MinimumKeyword(long min) : base(new LongTypeBenchmarkChecker(min))
    {
    }

    public MinimumKeyword(ulong min) : base(new UnsignedLongTypeBenchmarkChecker(min))
    {
    }

    [Obfuscation]
    public static string ErrorMessage(object instanceValue, object minimum)
    {
        return $"Instance '{instanceValue}' is less than '{minimum}'";
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
            return instanceValue >= _benchmark;
        }

        public override bool IsInRange(long instanceValue)
        {
            return instanceValue >= _benchmark;
        }

        public override bool IsInRange(ulong instanceValue)
        {
            return instanceValue >= _benchmark;
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
            return instanceValue >= _benchmark;
        }

        public override bool IsInRange(long instanceValue)
        {
            return instanceValue >= _benchmark;
        }

        public override bool IsInRange(ulong instanceValue)
        {
            if (_benchmark < 0)
            {
                return true;
            }
            return instanceValue >= (ulong)_benchmark;
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
            return instanceValue >= _benchmark;
        }

        public override bool IsInRange(long instanceValue)
        {
            if (_benchmark > long.MaxValue)
            {
                return false;
            }

            return instanceValue >= (long)_benchmark;
        }

        public override bool IsInRange(ulong instanceValue)
        {
            return instanceValue >= _benchmark;
        }

        public override string GetErrorMessage(object instanceValue)
        {
            return ErrorMessage(instanceValue, _benchmark);
        }
    }
}