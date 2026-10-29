using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

public class MetaSchemaRegistry
{
    private readonly Dictionary<Uri, MetaSchema> _metaSchemas = new();

    public void AddMetaSchema(string metaSchemaText)
    {
        MetaSchema metaSchema = JsonSerializer.Deserialize<MetaSchema>(metaSchemaText)!;
        _metaSchemas[metaSchema.Id] = metaSchema;
    }

    public MetaSchema? GetMetaSchema(Uri metaSchemaId)
    {
        return _metaSchemas.GetValueOrDefault(metaSchemaId);
    }
}