using System.Collections;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

public class JsonSchemaOptions
{
    internal SchemaResourceRegistry? SchemaResourceRegistry { get; set; }
    internal SchemaRecursionRecorder SchemaRecursionRecorder { get; } = new();
    public ValidationPathStack ValidationPathStack { get; } = new();

    /// <summary>
    /// Gets or sets a value that defines whether JSON schema validates with 'format' keyword.
    /// </summary>
    /// <returns>true if JSON schema should validate with 'format' keyword; otherwise, false. The default is true.</returns>
    public bool ValidateFormat { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that specifies how long a pattern matching method should attempt a match before it times out.
    /// </summary>
    /// <returns>A time-out interval, or <see cref="System.Text.RegularExpressions.Regex.InfiniteMatchTimeout"/> to indicate that the pattern matching method should not time out. The default is <see cref="RegexFactory.DefaultMatchTimeout"/>.</returns>
    public TimeSpan RegexMatchTimeout { get; set; } = RegexFactory.DefaultMatchTimeout;

    /// <summary>
    /// Gets or sets a value that specifies the format of validation output
    /// </summary>
    public OutputFormat OutputFormat { get; set; }

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
            RegexMatchTimeout = options.RegexMatchTimeout;
            OutputFormat = options.OutputFormat;
        }
    }
}

public enum OutputFormat
{
    /// <summary>
    /// Indicates that only first failed validation node found out will be returned.
    /// This will enable fast-fail pattern (eg. one of the cases is when one failed validation keyword is found, it may skip follow-up keywords validation inside same json schema)
    /// </summary>
    FailFast,

    /// <summary>
    /// Indicates that all failed validations will be returned
    /// </summary>
    List
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