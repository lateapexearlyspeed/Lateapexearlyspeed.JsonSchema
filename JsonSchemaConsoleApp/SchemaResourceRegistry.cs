namespace JsonSchemaConsoleApp;

internal class SchemaResourceRegistry
{
    private readonly Dictionary<Uri, JsonSchemaResource> _schemaResources = new();

    public JsonSchemaResource? GetSchemaResource(Uri baseUri)
    {
        return _schemaResources.GetValueOrDefault(baseUri);
    }

    public void AddSchemaResource(Uri absoluteBaseUri, JsonSchemaResource schemaResource)
    {
        _schemaResources.Add(absoluteBaseUri, schemaResource);
    }

    public void Add(SchemaResourceRegistry otherSchemaResourceRegistry)
    {
        foreach (KeyValuePair<Uri, JsonSchemaResource> otherKv in otherSchemaResourceRegistry._schemaResources)
        {
            _schemaResources.Add(otherKv.Key, otherKv.Value);
        }
    }
}