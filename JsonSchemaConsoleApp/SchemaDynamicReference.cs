using System.Text.Json;

namespace JsonSchemaConsoleApp;

internal class SchemaDynamicReference : NamedValidationNode
{
    public const string Keyword = "$dynamicRef";

    private Uri? _fullUriRef;
    private readonly SchemaReference _staticSchemaReference;

    public SchemaDynamicReference(Uri rawRefValue)
    {
        Name = Keyword;
        RawRefValue = rawRefValue;
        _staticSchemaReference = new SchemaReference(rawRefValue);
    }

    public Uri RawRefValue { get; }

    public Uri ParentResourceBaseUri
    {
        set
        {
            _fullUriRef = new Uri(value, RawRefValue);
            _staticSchemaReference.ParentResourceBaseUri = value;
        }
    }

    public JsonSchemaResource? GetReferencedSchemaResource(JsonSchemaOptions options)
    {
        JsonSchemaResource? staticReferencedSchemaResource = _staticSchemaReference.GetReferencedSchemaResource(options);
        if (staticReferencedSchemaResource is null)
        {
            return null;
        }

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

    private (JsonSchema? subSchema, Uri? subSchemaFullUriRef) GetReferencedSchema(JsonSchemaOptions options)
    {
        JsonSchemaResource? staticReferencedSchemaResource = _staticSchemaReference.GetReferencedSchemaResource(options);
        if (staticReferencedSchemaResource is null)
        {
            return (null, null);
        }

        string dynamicAnchor = _fullUriRef.UnescapedFragmentWithoutNumberSign();
        BodyJsonSchema? innerMostSubSchema = staticReferencedSchemaResource.FindSubSchemaByDynamicAnchor(dynamicAnchor);
        if (innerMostSubSchema is null)
        {
            // Fall back to apply static reference way
            JsonSchema? staticReferencedSchema = _staticSchemaReference.GetReferencedSchema(options);
            return staticReferencedSchema is null
                ? (null, null)
                : (staticReferencedSchema, _staticSchemaReference.FullUriRef);
        }

        foreach (var (resource, _) in options.ValidationPathStack.SchemaLocationStack.Reverse())
        {
            BodyJsonSchema? subSchema = resource.FindSubSchemaByDynamicAnchor(dynamicAnchor);

            if (subSchema is not null)
            {
                return (subSchema, new UriBuilder(resource.BaseUri){Fragment = _fullUriRef.Fragment }.Uri);
            }
        }

        // Cannot find specified dynamic anchor in reference path, so use the innermost subschema which contains specified dynamic anchor.
        return (innerMostSubSchema, new UriBuilder(staticReferencedSchemaResource.BaseUri){Fragment = _fullUriRef.Fragment }.Uri);
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        (JsonSchema? subSchema, Uri? subSchemaFullUriRef) referencedSchema = GetReferencedSchema(options);
        if (referencedSchema.subSchema is null)
        {
            throw new InvalidOperationException($"Cannot find schema for dynamic ref: {RawRefValue}");
        }

        if (!options.SchemaRecursionRecorder.TryAdd(referencedSchema.subSchema, JsonPath.Root))
        {
            throw new InvalidOperationException($"Infinite recursion loop detected. Instance path: {""}");
        }

        options.ValidationPathStack.PushReferencedSchema(GetReferencedSchemaResource(options)!, referencedSchema.subSchemaFullUriRef);

        ValidationResult validationResult = referencedSchema.subSchema.ValidateCore(instance, options);

        options.ValidationPathStack.PopReferencedSchema();

        return validationResult;
    }
}