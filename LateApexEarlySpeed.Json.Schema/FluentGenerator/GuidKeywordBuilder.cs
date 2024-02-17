using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class GuidKeywordBuilder : KeywordBuilder
{
    public GuidKeywordBuilder() : base(new FormatKeyword(GuidFormatValidator.FormatName))
    {
    }
}