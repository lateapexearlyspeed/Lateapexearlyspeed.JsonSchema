using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

public class MetaSchemaRegistry
{
    private readonly Dictionary<Uri, MetaSchema> _metaSchemas = new();

    /// <summary>
    /// Adds a meta schema to the registry.
    /// </summary>
    /// <param name="metaSchemaText">The JSON text of the meta schema.</param>
    public void AddMetaSchema(string metaSchemaText)
    {
        MetaSchema metaSchema = JsonSerializer.Deserialize<MetaSchema>(metaSchemaText)!;
        _metaSchemas[metaSchema.Id] = metaSchema;
    }

    internal MetaSchema? GetMetaSchema(Uri metaSchemaId)
    {
        return _metaSchemas.GetValueOrDefault(metaSchemaId);
    }
}