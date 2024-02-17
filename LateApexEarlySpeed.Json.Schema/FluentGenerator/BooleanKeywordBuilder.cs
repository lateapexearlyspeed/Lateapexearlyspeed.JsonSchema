using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

internal class BooleanKeywordBuilder : KeywordBuilder
{
    public BooleanKeywordBuilder() : base(new TypeKeyword(InstanceType.Boolean))
    {
    }
}