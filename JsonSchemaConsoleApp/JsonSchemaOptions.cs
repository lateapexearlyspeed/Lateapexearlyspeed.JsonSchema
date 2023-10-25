using System.Collections;
using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp;

public class JsonSchemaOptions
{
    public SchemaResourceRegistry SchemaResourceRegistry { get; init; }
    public SchemaRecursionRecorder SchemaRecursionRecorder { get; init; }
    public ValidationPathStack ValidationPathStack { get; init; }
}

public class ValidationPathStack
{
    public RelativeKeywordLocationStack RelativeKeywordLocationStack { get; } = new();

    public SchemaLocationStack SchemaLocationStack { get; } = new();

    public void PushRelativeLocation(string name)
    {
        RelativeKeywordLocationStack.Push(name);
    }

    public void PopRelativeLocation()
    {
        RelativeKeywordLocationStack.Pop();
    }

    public void PushReferencedSchema(JsonSchemaResource referencedResource, Uri subSchemaRefFullUri)
    {
        SchemaLocationStack.Push(referencedResource, subSchemaRefFullUri);
    }

    public void PopReferencedSchema()
    {
        SchemaLocationStack.Pop();
    }
}

public class SchemaLocationStack : IEnumerable<(JsonSchemaResource resource, Uri subSchemaRefFullUri)>
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

public class RelativeKeywordLocationStack
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

    public JsonPointer ToJsonPointer()
    {
        return new JsonPointer(_locationStack.Reverse());
    }
}