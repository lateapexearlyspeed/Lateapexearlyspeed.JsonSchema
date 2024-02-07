using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ExclusiveMinimumAttribute : Attribute, IKeywordGenerator
{
    private readonly double _minimum;

    public ExclusiveMinimumAttribute(double minimum)
    {
        _minimum = minimum;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new ExclusiveMinimumKeyword(_minimum);
    }
}