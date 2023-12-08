using System.Collections.Generic;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

internal interface ISubSchemaCollection
{
    List<JsonSchema> SubSchemas { get; init; }
}