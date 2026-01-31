using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class SchemaResourceRegistry
{
    private readonly List<JsonSchemaResource> _schemaResources;

    public SchemaResourceRegistry(int capacity)
    {
        _schemaResources = new List<JsonSchemaResource>(capacity);
    }

    public JsonSchemaResource? GetSchemaResource(Uri baseUri)
    {
        return _schemaResources.Find(resource => resource.BaseUri == baseUri);
    }

    public void AddSchemaResource(JsonSchemaResource schemaResource)
    {
        if (_schemaResources.Find(resource => resource.BaseUri == schemaResource.BaseUri) is not null)
        {
            throw new ArgumentException($"a schema resource with same base uri: {schemaResource.BaseUri} already exists.", nameof(schemaResource));
        }

        _schemaResources.Add(schemaResource);
    }

    public void AddSchemaResourcesFromRegistry(SchemaResourceRegistry otherSchemaResourceRegistry)
    {
        JsonSchemaResource? duplicatedResource = otherSchemaResourceRegistry._schemaResources.Find(otherResource => _schemaResources.Find(thisResource => thisResource.BaseUri == otherResource.BaseUri) is not null);

        if (duplicatedResource is not null)
        {
            throw new ArgumentException($"a schema resource with same base uri: {duplicatedResource.BaseUri} already exists.", nameof(otherSchemaResourceRegistry));
        }
        
        _schemaResources.AddRange(otherSchemaResourceRegistry._schemaResources);
    }
}