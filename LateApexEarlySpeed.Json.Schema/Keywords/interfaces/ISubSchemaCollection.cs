using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

internal interface ISubSchemaCollection
{
    IReadOnlyList<JsonSchema> SubSchemas { get; init; }
}