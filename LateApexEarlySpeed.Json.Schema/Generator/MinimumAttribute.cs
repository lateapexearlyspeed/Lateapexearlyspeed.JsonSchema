using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
public class MinimumAttribute : Attribute, IKeywordGenerator
{
    private readonly double _minimum;

    public MinimumAttribute(double minimum)
    {
        _minimum = minimum;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new MinimumKeyword(_minimum);
    }
}