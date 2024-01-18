using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
public class MaximumAttribute : Attribute, IKeywordGenerator
{
    private readonly double _maximum;

    public MaximumAttribute(double maximum)
    {
        _maximum = maximum;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new MaximumKeyword(_maximum);
    }
}