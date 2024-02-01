using System.Collections;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

public class JsonSchemaOptions
{
    internal SchemaResourceRegistry? SchemaResourceRegistry { get; set; }
    internal SchemaRecursionRecorder SchemaRecursionRecorder { get; } = new();
    public ValidationPathStack ValidationPathStack { get; } = new();
    public bool ValidateFormat { get; set; } = true;

    /// <summary>
    /// User entry point to create <see cref="JsonSchemaOptions"/> instance
    /// </summary>
    public JsonSchemaOptions()
    {
    }

    /// <summary>
    /// Copy user defined <paramref name="options"/> and set <see cref="SchemaResourceRegistry"/>
    /// </summary>
    internal JsonSchemaOptions(JsonSchemaOptions? options, SchemaResourceRegistry schemaResourceRegistry)
    {
        SchemaResourceRegistry = schemaResourceRegistry;

        if (options is not null)
        {
            ValidateFormat = options.ValidateFormat;
        }
    }
}

public class ValidationPathStack
{
    internal RelativeKeywordLocationStack RelativeKeywordLocationStack { get; } = new();

    internal ReferencedSchemaLocationStack ReferencedSchemaLocationStack { get; } = new();

    internal SchemaResourceStack SchemaResourceStack { get; } = new();

    internal void PushRelativeLocation(string name)
    {
        RelativeKeywordLocationStack.Push(name);
    }

    internal void PopRelativeLocation()
    {
        RelativeKeywordLocationStack.Pop();
    }

    internal void PushReferencedSchema(JsonSchemaResource referencedResource, Uri subSchemaRefFullUri)
    {
        ReferencedSchemaLocationStack.Push(referencedResource, subSchemaRefFullUri);
    }

    internal void PopReferencedSchema()
    {
        ReferencedSchemaLocationStack.Pop();
    }

    internal void PushSchemaResource(JsonSchemaResource schemaResource)
    {
        SchemaResourceStack.Push(schemaResource);
    }

    internal void PopSchemaResource()
    {
        SchemaResourceStack.Pop();
    }
}

internal class SchemaResourceStack : IEnumerable<JsonSchemaResource>
{
    private readonly Stack<JsonSchemaResource> _schemaResources = new();

    public void Push(JsonSchemaResource schemaResource)
    {
        _schemaResources.Push(schemaResource);
    }

    public void Pop()
    {
        _schemaResources.Pop();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<JsonSchemaResource> GetEnumerator()
    {
        return _schemaResources.GetEnumerator();
    }
}

internal class ReferencedSchemaLocationStack : IEnumerable<(JsonSchemaResource resource, Uri subSchemaRefFullUri)>
{
    private readonly Stack<(JsonSchemaResource resource, Uri subSchemaRefFullUri)> _schemaLocationStack = new();

    public void Push(JsonSchemaResource referencedResource, Uri subSchemaRefFullUri)
    {
        _schemaLocationStack.Push((referencedResource, subSchemaRefFullUri));
    }

    public void Pop()
    {
        _schemaLocationStack.Pop();
    }

    public (JsonSchemaResource resource, Uri subSchemaRefFullUri) Peek()
    {
        return _schemaLocationStack.Peek();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<(JsonSchemaResource resource, Uri subSchemaRefFullUri)> GetEnumerator()
    {
        return _schemaLocationStack.GetEnumerator();
    }
}

internal class RelativeKeywordLocationStack
{
    private readonly Stack<string> _locationStack = new();

    public void Push(string name)
    {
        _locationStack.Push(name);
    }

    public void Pop()
    {
        _locationStack.Pop();
    }

    public ImmutableJsonPointer ToJsonPointer()
    {
        return new ImmutableJsonPointer(_locationStack.Reverse());
    }
}