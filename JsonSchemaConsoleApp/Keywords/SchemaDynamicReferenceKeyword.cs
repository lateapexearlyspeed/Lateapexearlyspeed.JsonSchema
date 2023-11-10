using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SchemaDynamicReferenceKeywordJsonConverter))]
internal class SchemaDynamicReferenceKeyword : KeywordBase
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

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        (JsonSchema subSchema, Uri subSchemaFullUriRef)? referencedSchemaInfo = GetReferencedSchema(options);
        if (!referencedSchemaInfo.HasValue)
        {
            throw new InvalidOperationException($"Cannot find schema for {Keyword}: {RawRefValue}");
        }

        JsonSchema referencedSubSchema = referencedSchemaInfo.Value.subSchema;
        Uri subSchemaFullUriRef = referencedSchemaInfo.Value.subSchemaFullUriRef;

        if (!options.SchemaRecursionRecorder.TryAdd(referencedSubSchema, JsonPath.Root))
        {
            throw new InvalidOperationException($"Infinite recursion loop detected. Instance path: {""}");
        }

        JsonSchemaResource? referencedSchemaResource = GetReferencedSchemaResource(options);

        Debug.Assert(referencedSchemaResource is not null);
        options.ValidationPathStack.PushReferencedSchema(referencedSchemaResource, subSchemaFullUriRef);

        ValidationResult validationResult = referencedSubSchema.ValidateCore(instance, options);

        options.ValidationPathStack.PopReferencedSchema();

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
            // Fall back to apply static reference way
            JsonSchema? staticReferencedSchema = _staticSchemaReferenceKeyword.GetReferencedSchema(options);
            
            Debug.Assert(_staticSchemaReferenceKeyword.FullUriRef is not null);
            return staticReferencedSchema is null
                ? null
                : (staticReferencedSchema, _staticSchemaReferenceKeyword.FullUriRef);
        }

        foreach (var (resource, _) in options.ValidationPathStack.SchemaLocationStack.Reverse())
        {
            BodyJsonSchema? subSchema = resource.FindSubSchemaByDynamicAnchor(dynamicAnchor);

            if (subSchema is not null)
            {
                Debug.Assert(resource.BaseUri is not null);

                return (subSchema, new UriBuilder(resource.BaseUri) { Fragment = _fullUriRef.Fragment }.Uri);
            }
        }

        // Cannot find specified dynamic anchor in reference path, so use the innermost subschema which contains specified dynamic anchor.
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

        JsonSchemaResource? schemaResource = options.ValidationPathStack.SchemaLocationStack
            .Select(location => location.resource)
            .LastOrDefault(resource => resource.FindSubSchemaByDynamicAnchor(dynamicAnchor) is not null);
        return schemaResource ?? staticReferencedSchemaResource;
    }
}