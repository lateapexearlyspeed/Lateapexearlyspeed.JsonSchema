using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
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
        return new AllOfKeyword(new List<JsonSchema>
        {
            new BodyJsonSchema(new List<KeywordBase> { new MinimumKeyword(_min) }),
            new BodyJsonSchema(new List<KeywordBase> { new MaximumKeyword(_max) })
        });
    }
}