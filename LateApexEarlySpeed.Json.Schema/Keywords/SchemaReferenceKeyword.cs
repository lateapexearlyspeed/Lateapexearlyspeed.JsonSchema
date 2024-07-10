using System.Diagnostics;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SchemaReferenceKeywordJsonConverter))]
internal class SchemaReferenceKeyword : KeywordBase
{
    public const string Keyword = "$ref";

    private readonly Uri _rawRefValue;

    public SchemaReferenceKeyword(Uri rawRefValue)
    {
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

        Debug.Assert(FullUriRef is not null);

        string fragmentWithoutNumberSign = FullUriRef.UnescapedFragmentWithoutNumberSign();
        if (string.IsNullOrEmpty(fragmentWithoutNumberSign))
        {
            return schemaResource;
        }

        JsonSchema? schemaByJsonPointer = schemaResource.FindSubSchemaByJsonPointer(fragmentWithoutNumberSign);
        if (schemaByJsonPointer is not null)
        {
            return schemaByJsonPointer;
        }

        BodyJsonSchema? schemaByAnchor = schemaResource.FindSubSchemaByAnchor(fragmentWithoutNumberSign);
        if (schemaByAnchor is not null)
        {
            return schemaByAnchor;
        }

        // 'ref' keyword also checks '$dynamicAnchor',
        // based on test case "A $ref to a $dynamicAnchor in the same schema resource behaves like a normal $ref to an $anchor"
        return schemaResource.FindSubSchemaByDynamicAnchor(fragmentWithoutNumberSign);
    }

    public JsonSchemaResource? GetReferencedSchemaResource(JsonSchemaOptions options)
    {
        Debug.Assert(FullUriRef is not null);
        Debug.Assert(options.SchemaResourceRegistry is not null);

        return options.SchemaResourceRegistry.GetSchemaResource(GetBaseUri(FullUriRef));
    }

    /// <returns>Uri from <paramref name="fullUri"/> without fragment</returns>
    private static string GetBaseUri(Uri fullUri)
    {
        string baseUri = fullUri.GetLeftPart(UriPartial.Query);
        // return new Uri(baseUri);
        return baseUri;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(FullUriRef is not null);

        JsonSchema? referencedSchema = GetReferencedSchema(options);

        if (referencedSchema is null)
        {
            throw new InvalidOperationException($"Cannot find schema for {Keyword}: {FullUriRef}");
        }

        if (!options.SchemaRecursionRecorder.TryPushRecord(referencedSchema, instance.Location))
        {
            throw new InvalidOperationException($"Infinite recursion loop detected. Instance path: {instance.Location}");
        }

        JsonSchemaResource? referencedSchemaResource = GetReferencedSchemaResource(options);
        
        Debug.Assert(referencedSchemaResource is not null);
        options.ValidationPathStack.PushSchemaResource(referencedSchemaResource);
        options.ValidationPathStack.PushReferencedSchema(referencedSchemaResource, FullUriRef);

        ValidationResult validationResult = referencedSchema.ValidateCore(instance, options);

        options.ValidationPathStack.PopReferencedSchema();
        options.ValidationPathStack.PopSchemaResource();
        options.SchemaRecursionRecorder.PopRecord();

        return validationResult;
    }
}