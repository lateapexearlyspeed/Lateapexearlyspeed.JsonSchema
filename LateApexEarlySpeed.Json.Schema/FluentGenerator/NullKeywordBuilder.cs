using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class NullKeywordBuilder : KeywordBuilder
{
    public NullKeywordBuilder() : base(new TypeKeyword(InstanceType.Null))
    {
    }
}