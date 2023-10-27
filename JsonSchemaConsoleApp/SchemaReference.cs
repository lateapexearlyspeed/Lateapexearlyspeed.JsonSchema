using System.IO;
using System.Text.Json;

namespace JsonSchemaConsoleApp;

public class SchemaReference : ValidationNode
{
    public const string Keyword = "$ref";

    private readonly Uri _rawRefValue;

    public SchemaReference(Uri rawRefValue)
    {
        Name = Keyword;
        _rawRefValue = rawRefValue;
    }

    public Uri? FullUriRef { get; private set; }

    public Uri ParentResourceBaseUri
    {
        set => FullUriRef = new Uri(value, _rawRefValue);
    }

    public JsonSchema? GetReferencedSchema(JsonSchemaOptions options)
    {
        JsonSchemaResource? schemaResource = GetReferencedSchemaResource(options);

        if (schemaResource is null)
        {
            return null;
        }

        string fragmentWithoutNumberSign = FullUriRef.Fragment.TrimStart('#');
        if (string.IsNullOrEmpty(fragmentWithoutNumberSign))
        {
            return schemaResource;
        }

        JsonSchema? schemaByJsonPointer = schemaResource.FindSubSchemaByJsonPointer(fragmentWithoutNumberSign);
        if (schemaByJsonPointer is not null)
        {
            return schemaByJsonPointer;
        }


        JsonSchema? schemaFromDefs = schemaResource.FindSubSchemaByDefs(fragmentWithoutNumberSign);
        if (schemaFromDefs is not null)
        {
            return schemaFromDefs;
        }

        return schemaResource.FindSubSchemaByAnchor(fragmentWithoutNumberSign);
    }

    public JsonSchemaResource? GetReferencedSchemaResource(JsonSchemaOptions options)
    {
        return options.SchemaResourceRegistry.GetSchemaResource(GetBaseUri(FullUriRef));
    }

    private static Uri GetBaseUri(Uri fullUri)
    {
        string baseUri = fullUri.AbsoluteUri.Substring(0, fullUri.AbsoluteUri.Length - fullUri.Fragment.Length);
        return new Uri(baseUri);
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        JsonSchema? referencedSchema = GetReferencedSchema(options);

        if (referencedSchema is null)
        {
            throw new InvalidOperationException($"Cannot find schema for ref: {FullUriRef}");
        }

        if (!options.SchemaRecursionRecorder.TryAdd(referencedSchema, JsonPath.Root))
        {
            throw new InvalidOperationException($"Infinite recursion loop detected. Instance path: {""}");
        }

        options.ValidationPathStack.PushReferencedSchema(GetReferencedSchemaResource(options)!, FullUriRef);

        ValidationResult validationResult = referencedSchema.ValidateCore(instance, options);

        options.ValidationPathStack.PopReferencedSchema();

        return validationResult;
    }
}