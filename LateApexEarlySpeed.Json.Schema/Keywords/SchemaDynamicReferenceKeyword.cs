using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SchemaDynamicReferenceKeywordJsonConverter))]
internal class SchemaDynamicReferenceKeyword : KeywordBase, IReferenceKeyword
{
    public const string Keyword = "$dynamicRef";

    private Uri? _fullUriRef;
    private readonly SchemaReferenceKeyword _staticSchemaReferenceKeyword;

    public SchemaDynamicReferenceKeyword(Uri rawRefValue)
    {
        RawRefValue = rawRefValue;
        _staticSchemaReferenceKeyword = new SchemaReferenceKeyword(rawRefValue);
    }

    public Uri RawRefValue { get; }

    public Uri ParentResourceBaseUri
    {
        set
        {
            _fullUriRef = new Uri(value, RawRefValue);
            _staticSchemaReferenceKeyword.ParentResourceBaseUri = value;
        }
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        (JsonSchema subSchema, Uri subSchemaFullUriRef)? referencedSchemaInfo = GetReferencedSchema(options);
        if (!referencedSchemaInfo.HasValue)
        {
            throw new InvalidOperationException($"Cannot find schema for {Keyword}: {RawRefValue}");
        }

        JsonSchema referencedSubSchema = referencedSchemaInfo.Value.subSchema;
        Uri subSchemaFullUriRef = referencedSchemaInfo.Value.subSchemaFullUriRef;

        if (!options.SchemaRecursionRecorder.TryPushRecord(referencedSubSchema, instance.Location))
        {
            throw new InvalidOperationException($"Infinite recursion loop detected. Instance path: {instance.Location}");
        }

        JsonSchemaResource? referencedSchemaResource = GetReferencedSchemaResource(options);

        Debug.Assert(referencedSchemaResource is not null);
        options.ValidationPathStack.PushSchemaResource(referencedSchemaResource);
        options.ValidationPathStack.PushReferencedSchema(referencedSchemaResource, subSchemaFullUriRef);

        ValidationResult validationResult = referencedSubSchema.ValidateCore(instance, options);

        options.ValidationPathStack.PopReferencedSchema();
        options.ValidationPathStack.PopSchemaResource();
        options.SchemaRecursionRecorder.PopRecord();

        return validationResult;
    }

    private (JsonSchema subSchema, Uri subSchemaFullUriRef)? GetReferencedSchema(JsonSchemaOptions options)
    {
        JsonSchemaResource? staticReferencedSchemaResource = _staticSchemaReferenceKeyword.GetReferencedSchemaResource(options);
        if (staticReferencedSchemaResource is null)
        {
            return null;
        }

        Debug.Assert(_fullUriRef is not null);

        string dynamicAnchor = _fullUriRef.UnescapedFragmentWithoutNumberSign();
        BodyJsonSchema? innerMostSubSchema = staticReferencedSchemaResource.FindSubSchemaByDynamicAnchor(dynamicAnchor);
        if (innerMostSubSchema is null)
        {
            Debug.Assert(_staticSchemaReferenceKeyword.FullUriRef is not null);

            // Fall back to apply static reference way
            return _staticSchemaReferenceKeyword.TryGetReferencedSchema(options, out JsonSchema? staticReferencedSchema, out _)
                ? (staticReferencedSchema, _staticSchemaReferenceKeyword.FullUriRef)
                : null;
        }

        foreach (JsonSchemaResource resource in options.ValidationPathStack.SchemaResourceStack.Reverse())
        {
            BodyJsonSchema? subSchema = resource.FindSubSchemaByDynamicAnchor(dynamicAnchor);

            if (subSchema is not null)
            {
                Debug.Assert(resource.BaseUri is not null);

                return (subSchema, new UriBuilder(resource.BaseUri) { Fragment = _fullUriRef.Fragment }.Uri);
            }
        }

        // Cannot find specified dynamic anchor in dynamic schema resources path, so use the innermost subschema which contains specified dynamic anchor.
        Debug.Assert(staticReferencedSchemaResource.BaseUri is not null);
        return (innerMostSubSchema, new UriBuilder(staticReferencedSchemaResource.BaseUri) { Fragment = _fullUriRef.Fragment }.Uri);
    }

    private JsonSchemaResource? GetReferencedSchemaResource(JsonSchemaOptions options)
    {
        JsonSchemaResource? staticReferencedSchemaResource = _staticSchemaReferenceKeyword.GetReferencedSchemaResource(options);
        if (staticReferencedSchemaResource is null)
        {
            return null;
        }

        Debug.Assert(_fullUriRef is not null);

        string dynamicAnchor = _fullUriRef.UnescapedFragmentWithoutNumberSign();
        BodyJsonSchema? schema = staticReferencedSchemaResource.FindSubSchemaByDynamicAnchor(dynamicAnchor);
        if (schema is null)
        {
            return staticReferencedSchemaResource;
        }

        JsonSchemaResource? schemaResource = options.ValidationPathStack.SchemaResourceStack
            .LastOrDefault(resource => resource.FindSubSchemaByDynamicAnchor(dynamicAnchor) is not null);
        return schemaResource ?? staticReferencedSchemaResource;
    }
}