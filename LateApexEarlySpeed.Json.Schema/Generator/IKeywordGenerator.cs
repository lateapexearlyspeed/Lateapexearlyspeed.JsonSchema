using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

internal interface IKeywordGenerator
{
    public ValidationKeywordBase CreateKeyword(Type type);
}