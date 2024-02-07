using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class StringEnumAttribute : Attribute, IKeywordGenerator
{
    private readonly EnumGenerator<string> _enumGenerator;

    public StringEnumAttribute(params string[] allowedValues)
    {
        _enumGenerator = new EnumGenerator<string>(allowedValues);
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return _enumGenerator.CreateKeyword();
    }
}