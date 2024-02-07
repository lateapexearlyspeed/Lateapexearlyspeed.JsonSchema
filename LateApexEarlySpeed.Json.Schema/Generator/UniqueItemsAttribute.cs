using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class UniqueItemsAttribute : Attribute, IKeywordGenerator
{
    public KeywordBase CreateKeyword(Type type)
    {
        return new UniqueItemsKeyword(true);
    }
}