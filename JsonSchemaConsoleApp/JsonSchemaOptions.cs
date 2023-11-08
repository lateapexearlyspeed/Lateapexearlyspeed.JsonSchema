using System.Collections;

namespace JsonSchemaConsoleApp;

public class JsonSchemaOptions
{
    internal SchemaResourceRegistry SchemaResourceRegistry { get; }
    internal SchemaRecursionRecorder SchemaRecursionRecorder { get; }
    public ValidationPathStack ValidationPathStack { get; }

    internal JsonSchemaOptions(SchemaResourceRegistry schemaResourceRegistry)
    {
        SchemaResourceRegistry = schemaResourceRegistry;

        SchemaRecursionRecorder = new SchemaRecursionRecorder();
        ValidationPathStack = new ValidationPathStack();
    }
}

public class ValidationPathStack
{
    internal RelativeKeywordLocationStack RelativeKeywordLocationStack { get; } = new();

    internal SchemaLocationStack SchemaLocationStack { get; } = new();

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
        SchemaLocationStack.Push(referencedResource, subSchemaRefFullUri);
    }

    internal void PopReferencedSchema()
    {
        SchemaLocationStack.Pop();
    }
}

internal class SchemaLocationStack : IEnumerable<(JsonSchemaResource resource, Uri subSchemaRefFullUri)>
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

    public JsonPointer ToJsonPointer()
    {
        return new JsonPointer(_locationStack.Reverse());
    }
}