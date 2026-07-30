using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

internal abstract class AnnotationKeywordBase
{
    protected AnnotationKeywordBase()
    {
        Type type = GetType();

        Name = KeywordHelper.GetKeywordName(type);
    }

    public string Name { get; }

    public abstract AnnotationCollection Annotate(JsonInstanceElement instance, JsonSchemaOptions options);
}

internal class AnnotationCollection
{
}

internal class Annotation
{
    public Annotation(ImmutableJsonPointer instanceLocation, ImmutableJsonPointer relativeKeywordLocation, Uri schemaResourceBaseUri, Uri subSchemaRefFullUri, string keyword, JsonElement value)
    {
        InstanceLocation = instanceLocation;
        RelativeKeywordLocation = relativeKeywordLocation;
        SchemaResourceBaseUri = schemaResourceBaseUri;
        SubSchemaRefFullUri = subSchemaRefFullUri;
        Keyword = keyword;
        Value = value;
    }

    /// <summary>
    /// Gets value to indicate json instance's location
    /// </summary>
    public ImmutableJsonPointer InstanceLocation { get; }

    /// <summary>
    /// Gets value to indicate relative location of keyword
    /// </summary>
    public ImmutableJsonPointer RelativeKeywordLocation { get; }

    /// <summary>
    /// Gets value to indicate base uri of current json schema resource
    /// </summary>
    public Uri SchemaResourceBaseUri { get; }

    /// <summary>
    /// Gets value to indicate full uri of referenced sub-schema
    /// </summary>
    public Uri SubSchemaRefFullUri { get; }

    /// <summary>
    /// Gets value to indicate current keyword.
    /// </summary>
    public string Keyword { get; }

    /// <summary>
    /// The value of annotation, which is the value of the keyword in the json schema.
    /// </summary>
    public JsonElement Value { get; }
}