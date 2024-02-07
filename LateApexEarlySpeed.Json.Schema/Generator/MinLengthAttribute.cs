using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MinLengthAttribute : Attribute, IKeywordGenerator
{
    private readonly uint _min;

    public MinLengthAttribute(uint min)
    {
        _min = min;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return type == typeof(string)
            ? new MinLengthKeyword { BenchmarkValue = _min }
            : new MinItemsKeyword() { BenchmarkValue = _min };
    }
}