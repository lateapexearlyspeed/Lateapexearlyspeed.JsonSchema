using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

internal class FalseKeywordBuilder : KeywordBuilder
{
    public FalseKeywordBuilder() : base(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(false)))
    {
    }
}