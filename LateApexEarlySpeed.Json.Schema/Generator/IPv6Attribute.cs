using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
// ReSharper disable once InconsistentNaming
public class IPv6Attribute : Attribute, IKeywordGenerator
{
    public ValidationKeywordBase CreateKeyword(Type type)
    {
        return new FormatKeyword(IPv6FormatValidator.FormatName, new IPv6FormatValidator());
    }
}