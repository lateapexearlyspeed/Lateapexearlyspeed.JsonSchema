using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class StringEnumAttribute : Attribute, IKeywordGenerator
{
    private readonly EnumKeywordGenerator<string> _enumKeywordGenerator;

    public StringEnumAttribute(params string[] allowedValues)
    {
        _enumKeywordGenerator = new EnumKeywordGenerator<string>(allowedValues);
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return _enumKeywordGenerator.CreateKeyword();
    }
}