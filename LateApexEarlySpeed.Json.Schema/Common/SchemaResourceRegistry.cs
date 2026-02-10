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
        foreach (JsonSchemaResource schemaResource in _schemaResources)
        {
            if (schemaResource.BaseUri == baseUri)
            {
                return schemaResource;
            }
        }

        return null;
    }

    public void AddSchemaResource(JsonSchemaResource schemaResource)
    {
        foreach (JsonSchemaResource resource in _schemaResources)
        {
            if (resource.BaseUri == schemaResource.BaseUri)
            {
                throw new ArgumentException($"a schema resource with same base uri: {schemaResource.BaseUri} already exists.", nameof(schemaResource));
            }
        }

        _schemaResources.Add(schemaResource);
    }

    public void AddSchemaResourcesFromRegistry(SchemaResourceRegistry otherSchemaResourceRegistry)
    {
        foreach (JsonSchemaResource otherResource in otherSchemaResourceRegistry._schemaResources)
        {
            foreach (JsonSchemaResource resource in _schemaResources)
            {
                if (resource.BaseUri == otherResource.BaseUri)
                {
                    throw new ArgumentException($"a schema resource with same base uri: {otherResource.BaseUri} already exists.", nameof(otherSchemaResourceRegistry));
                }
            }
        }

        _schemaResources.AddRange(otherSchemaResourceRegistry._schemaResources);
    }
}