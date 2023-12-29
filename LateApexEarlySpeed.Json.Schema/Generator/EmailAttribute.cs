using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
public class EmailAttribute : Attribute, IKeywordGenerator
{
    public KeywordBase CreateKeyword(Type type)
    {
        return new FormatKeyword(EmailFormatValidator.FormatName);
    }
}