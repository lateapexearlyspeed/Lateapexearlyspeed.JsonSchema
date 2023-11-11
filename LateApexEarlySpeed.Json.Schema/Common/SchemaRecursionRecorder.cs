using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class SchemaRecursionRecorder
{
    private readonly HashSet<(JsonSchema schema, JsonPath instancePath)> _schemaInstanceCollection = new();

    public bool TryAdd(JsonSchema schema, JsonPath instancePath)
        => _schemaInstanceCollection.Add((schema, instancePath));
}