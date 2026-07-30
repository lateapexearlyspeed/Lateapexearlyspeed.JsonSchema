using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class LengthRangeAttribute : Attribute, IKeywordGenerator
{
    private readonly uint _min;
    private readonly uint _max;

    public LengthRangeAttribute(uint min, uint max)
    {
        _min = min;
        _max = max;
    }

    public ValidationKeywordBase CreateKeyword(Type type)
    {
        return new AllOfKeyword(new []
        {
            new BodyJsonSchema(new[] { CreateKeywordForMin(type) }),
            new BodyJsonSchema(new[] { CreateKeywordForMax(type) })
        });
    }

    private ValidationKeywordBase CreateKeywordForMax(Type type)
    {
        return type == typeof(string)
            ? new MaxLengthKeyword { BenchmarkValue = _max }
            : new MaxItemsKeyword { BenchmarkValue = _max };
    }

    private ValidationKeywordBase CreateKeywordForMin(Type type)
    {
        return type == typeof(string)
            ? new MinLengthKeyword { BenchmarkValue = _min }
            : new MinItemsKeyword { BenchmarkValue = _min };
    }
}