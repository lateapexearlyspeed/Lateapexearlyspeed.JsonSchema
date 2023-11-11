using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

internal interface ISingleSubSchema
{
    JsonSchema Schema { get; init; }
}