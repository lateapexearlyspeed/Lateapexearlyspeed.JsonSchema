using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class SchemaResourceRegistry
{
    private readonly Dictionary<Uri, JsonSchemaResource> _schemaResources = new();

    public JsonSchemaResource? GetSchemaResource(Uri baseUri)
    {
        return _schemaResources.GetValueOrDefault(baseUri);
    }

    public void AddSchemaResource(JsonSchemaResource schemaResource)
    {
        Debug.Assert(schemaResource.BaseUri is not null);

        _schemaResources.Add(schemaResource.BaseUri, schemaResource);
    }

    public void AddSchemaResourcesFromRegistry(SchemaResourceRegistry otherSchemaResourceRegistry)
    {
        foreach (KeyValuePair<Uri, JsonSchemaResource> otherKv in otherSchemaResourceRegistry._schemaResources)
        {
            _schemaResources.Add(otherKv.Key, otherKv.Value);
        }
    }
}