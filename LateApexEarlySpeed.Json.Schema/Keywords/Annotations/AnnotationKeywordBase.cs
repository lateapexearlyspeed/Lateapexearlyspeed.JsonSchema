using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

public abstract class AnnotationKeywordBase
{
    protected AnnotationKeywordBase()
    {
        Type type = GetType();

        Name = KeywordHelper.GetKeywordName(type);
    }

    public string Name { get; }

    public Annotation Annotate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        options.ValidationPathStack.PushRelativeLocation(Name);

        Annotation annotation = AnnotateCore(instance, options);

        options.ValidationPathStack.PopRelativeLocation();

        return annotation;
    }

    public abstract Annotation AnnotateCore(JsonInstanceElement instance, JsonSchemaOptions options);
}

public class AnnotationKeyword<T> : AnnotationKeywordBase
{
    public T Value { get; init; } = default!;

    public override Annotation AnnotateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return new Annotation(Name, JsonSerializer.SerializeToElement(Value), instance.Location, options.ValidationPathStack);
    }
}

public class Annotation
{
    public Annotation(string keyword, JsonElement value, ImmutableJsonPointer instanceLocation, ValidationPathStack validationPathStack)
    {
        Keyword = keyword;
        Value = value;
        InstanceLocation = instanceLocation;

        RelativeKeywordLocation = validationPathStack.RelativeKeywordLocationStack.ToJsonPointer();
        SchemaResourceBaseUri = validationPathStack.ReferencedSchemaLocationStack.Peek().resource.BaseUri!;
        SubSchemaRefFullUri = validationPathStack.ReferencedSchemaLocationStack.Peek().subSchemaRefFullUri;
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