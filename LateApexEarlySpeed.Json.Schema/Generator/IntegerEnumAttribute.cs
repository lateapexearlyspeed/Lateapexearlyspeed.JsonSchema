using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
public class IntegerEnumAttribute : Attribute, IKeywordGenerator
{
    private readonly EnumGenerator<int> _enumGenerator;

    public IntegerEnumAttribute(params int[] allowedValues)
    {
        _enumGenerator = new EnumGenerator<int>(allowedValues);
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return _enumGenerator.CreateKeyword();
    }
}