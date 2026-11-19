using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class SchemaResourceRegistry
{
    private object? _lock;

    private readonly List<(Uri baseOrRetrievalUri, JsonSchemaResource schemaResource)> _schemaResources;

    public SchemaResourceRegistry(int capacity)
    {
        _schemaResources = new List<(Uri baseOrRetrievalUri, JsonSchemaResource schemaResource)>(capacity);
    }

    public JsonSchemaResource? GetSchemaResource(Uri baseUri)
    {
        if (EnableConcurrency)
        {
            Monitor.Enter(_lock);
        }

        try
        {
            foreach ((Uri baseOrRetrievalUri, JsonSchemaResource schemaResource) in _schemaResources)
            {
                if (baseOrRetrievalUri == baseUri)
                {
                    return schemaResource;
                }
            }

            return null;
        }
        finally
        {
            if (EnableConcurrency)
            {
                Monitor.Exit(_lock);
            }
        }
    }

    public void AddSchemaResource(JsonSchemaResource schemaResource)
    {
        Debug.Assert(schemaResource.BaseUri is not null);

        AddSchemaResource(schemaResource.BaseUri, schemaResource);
    }

    public void AddSchemaResource(Uri baseOrRetrievalUri, JsonSchemaResource schemaResource)
    {
        if (EnableConcurrency)
        {
            Monitor.Enter(_lock);
        }

        try
        {
            foreach ((Uri baseOrRetrievalUri, JsonSchemaResource) resourceKv in _schemaResources)
            {
                if (resourceKv.baseOrRetrievalUri == baseOrRetrievalUri)
                {
                    throw new ArgumentException($"a schema resource with same uri: {baseOrRetrievalUri} already exists.", nameof(baseOrRetrievalUri));
                }
            }

            _schemaResources.Add((baseOrRetrievalUri, schemaResource));
        }
        finally
        {
            if (EnableConcurrency)
            {
                Monitor.Exit(_lock);
            }
        }
    }

    public void AddSchemaResourcesFromRegistry(SchemaResourceRegistry otherSchemaResourceRegistry)
    {
        if (EnableConcurrency)
        {
            Monitor.Enter(_lock);
        }

        try
        {
            foreach ((Uri otherBaseOrRetrievalUri, _) in otherSchemaResourceRegistry._schemaResources)
            {
                foreach ((Uri baseOrRetrievalUri, _) in _schemaResources)
                {
                    if (baseOrRetrievalUri == otherBaseOrRetrievalUri)
                    {
                        throw new ArgumentException($"a schema resource with same base uri (or retrievalUri): {otherBaseOrRetrievalUri} already exists.", nameof(otherSchemaResourceRegistry));
                    }
                }
            }

            _schemaResources.AddRange(otherSchemaResourceRegistry._schemaResources);
        }
        finally
        {
            if (EnableConcurrency)
            {
                Monitor.Exit(_lock);
            }
        }
    }

    public LockGenerator<Uri>? LockGenerator { get; private set; }

    public bool EnableConcurrency
    {
        get => _lock is not null;
        set
        {
            if (value)
            {
                _lock = new object();
                LockGenerator = new LockGenerator<Uri>();
            }
            else
            {
                _lock = null;
                LockGenerator = null;
            }
        }
    }
}