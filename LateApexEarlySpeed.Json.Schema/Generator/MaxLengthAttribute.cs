using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MaxLengthAttribute : Attribute, IKeywordGenerator
{
    private readonly uint _max;

    public MaxLengthAttribute(uint max)
    {
        _max = max;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return type == typeof(string)
            ? new MaxLengthKeyword { BenchmarkValue = _max }
            : new MaxItemsKeyword { BenchmarkValue = _max };
    }
}