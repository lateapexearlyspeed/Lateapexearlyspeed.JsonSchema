using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IntegerEnumAttribute : Attribute, IKeywordGenerator
{
    private readonly EnumKeywordGenerator<int> _enumKeywordGenerator;

    public IntegerEnumAttribute(params int[] allowedValues)
    {
        _enumKeywordGenerator = new EnumKeywordGenerator<int>(allowedValues);
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return _enumKeywordGenerator.CreateKeyword();
    }
}