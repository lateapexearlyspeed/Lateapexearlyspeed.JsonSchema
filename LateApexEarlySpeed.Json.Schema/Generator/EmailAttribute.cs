using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class EmailAttribute : Attribute, IKeywordGenerator
{
    public ValidationKeywordBase CreateKeyword(Type type)
    {
        return new FormatKeyword(EmailFormatValidator.FormatName, new EmailFormatValidator());
    }
}