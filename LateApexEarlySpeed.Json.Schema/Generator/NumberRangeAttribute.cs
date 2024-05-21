using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NumberRangeAttribute : Attribute, IKeywordGenerator
{
    private readonly double _min;
    private readonly double _max;

    public NumberRangeAttribute(double min, double max)
    {
        _min = min;
        _max = max;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new AllOfKeyword(new[]
        {
            new BodyJsonSchema(new[] { new MinimumKeyword(_min) }),
            new BodyJsonSchema(new[] { new MaximumKeyword(_max) })
        });
    }
}