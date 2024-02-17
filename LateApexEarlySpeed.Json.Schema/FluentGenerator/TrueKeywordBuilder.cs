using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

internal class TrueKeywordBuilder : KeywordBuilder
{
    public TrueKeywordBuilder() : base(new ConstKeyword(JsonInstanceSerializer.SerializeToElement(true)))
    {
    }
}