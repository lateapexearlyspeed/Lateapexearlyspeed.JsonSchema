using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

internal interface IKeywordGenerator
{
    public KeywordBase CreateKeyword(Type type);
}