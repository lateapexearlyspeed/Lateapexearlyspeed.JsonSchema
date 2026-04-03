using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class SchemaResourceRegistry
{
    private object? _lock;

    private readonly List<JsonSchemaResource> _schemaResources;

    public SchemaResourceRegistry(int capacity)
    {
        _schemaResources = new List<JsonSchemaResource>(capacity);
    }

    public JsonSchemaResource? GetSchemaResource(Uri baseUri)
    {
        if (EnableConcurrency)
        {
            Monitor.Enter(_lock);
        }

        try
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
        if (EnableConcurrency)
        {
            Monitor.Enter(_lock);
        }

        try
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