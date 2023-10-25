using System.Text.Json;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp;

public class SchemaRecursionRecorder
{
    private readonly HashSet<(JsonSchema schema, JsonPath instancePath)> _schemaInstanceCollection = new();

    public bool TryAdd(JsonSchema schema, JsonPath instancePath) 
        => _schemaInstanceCollection.Add((schema, instancePath));
}