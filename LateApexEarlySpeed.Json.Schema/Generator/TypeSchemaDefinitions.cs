using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Generator;

internal class TypeSchemaDefinitions
{
    private readonly Dictionary<string, JsonSchemaResource> _schemaResourceDefinitions = new();

    public JsonSchemaResource? GetSchemaDefinition(Type type)
    {
        return _schemaResourceDefinitions.GetValueOrDefault(GetDefName(type));
    }

    public void AddSchemaDefinition(Type type, JsonSchemaResource schemaResource)
    {
        _schemaResourceDefinitions.TryAdd(GetDefName(type), schemaResource);
    }

    public Dictionary<string, JsonSchemaResource> GetAll()
    {
        return _schemaResourceDefinitions;
    }

    public static string GetDefName(Type type)
    {
        return type.FullName!;
    }
}