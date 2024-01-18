using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
public class ExclusiveMaximumAttribute : Attribute, IKeywordGenerator
{
    private readonly double _maximum;

    public ExclusiveMaximumAttribute(double maximum)
    {
        _maximum = maximum;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new ExclusiveMaximumKeyword(_maximum);
    }
}