using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MultipleOfAttribute : Attribute, IKeywordGenerator
{
    private readonly double _multipleOf;

    public MultipleOfAttribute(double multipleOf)
    {
        _multipleOf = multipleOf;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new MultipleOfKeyword(_multipleOf);
    }
}