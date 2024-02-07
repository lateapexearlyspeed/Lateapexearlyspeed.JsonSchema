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

    public KeywordBase CreateKeyword(Type type)
    {
        return new AllOfKeyword(new List<JsonSchema>
        {
            new BodyJsonSchema(new List<KeywordBase> { CreateKeywordForMin(type) }),
            new BodyJsonSchema(new List<KeywordBase> { CreateKeywordForMax(type) })
        });
    }

    private KeywordBase CreateKeywordForMax(Type type)
    {
        return type == typeof(string)
            ? new MaxLengthKeyword { BenchmarkValue = _max }
            : new MaxItemsKeyword { BenchmarkValue = _max };
    }

    private KeywordBase CreateKeywordForMin(Type type)
    {
        return type == typeof(string)
            ? new MinLengthKeyword { BenchmarkValue = _min }
            : new MinItemsKeyword { BenchmarkValue = _min };
    }
}