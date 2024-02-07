using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
// ReSharper disable once InconsistentNaming
public class IPv4Attribute : Attribute, IKeywordGenerator
{
    public KeywordBase CreateKeyword(Type type)
    {
        return new FormatKeyword(IPv4FormatValidator.FormatName);
    }
}