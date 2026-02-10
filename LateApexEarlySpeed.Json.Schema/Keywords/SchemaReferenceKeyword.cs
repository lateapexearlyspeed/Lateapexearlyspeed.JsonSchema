using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SchemaReferenceKeywordJsonConverter))]
internal class SchemaReferenceKeyword : KeywordBase, IReferenceKeyword
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

    public bool TryGetReferencedSchema(JsonSchemaOptions options, [NotNullWhen(true)] out JsonSchema? referencedSchema, [NotNullWhen(true)] out JsonSchemaResource? referencedSchemaResource)
    {
        referencedSchemaResource = GetReferencedSchemaResource(options);

        if (referencedSchemaResource is null)
        {
            referencedSchema = null;
            return false;
        }

        Debug.Assert(FullUriRef is not null);

        string fragmentWithoutNumberSign = FullUriRef.UnescapedFragmentWithoutNumberSign();
        if (string.IsNullOrEmpty(fragmentWithoutNumberSign))
        {
            referencedSchema = referencedSchemaResource;
            return true;
        }

        referencedSchema = referencedSchemaResource.FindSubSchemaByJsonPointer(fragmentWithoutNumberSign);
        if (referencedSchema is not null)
        {
            return true;
        }

        referencedSchema = referencedSchemaResource.FindSubSchemaByPlainNameIdentifier(fragmentWithoutNumberSign);
        if (referencedSchema is not null)
        {
            return true;
        }

        // 'ref' keyword also checks '$dynamicAnchor',
        // based on test case "A $ref to a $dynamicAnchor in the same schema resource behaves like a normal $ref to an $anchor"
        referencedSchema = referencedSchemaResource.FindSubSchemaByDynamicAnchor(fragmentWithoutNumberSign);
        return referencedSchema is not null;
    }

    public JsonSchemaResource? GetReferencedSchemaResource(JsonSchemaOptions options)
    {
        Debug.Assert(FullUriRef is not null);
        Debug.Assert(options.SchemaResourceRegistry is not null);

        return options.SchemaResourceRegistry.GetSchemaResource(GetBaseUri(FullUriRef));
    }

    /// <returns>Uri from <paramref name="fullUri"/> without fragment</returns>
    private static Uri GetBaseUri(Uri fullUri)
    {
        string baseUri = fullUri.GetLeftPart(UriPartial.Query);
        return new Uri(baseUri);
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(FullUriRef is not null);

        if (!TryGetReferencedSchema(options, out JsonSchema? referencedSchema, out JsonSchemaResource? referencedSchemaResource))
        {
            throw new InvalidOperationException($"Cannot find schema for {Keyword}: {FullUriRef}");
        }

        if (!options.SchemaRecursionRecorder.TryPushRecord(referencedSchema, instance.Location))
        {
            throw new InvalidOperationException($"Infinite recursion loop detected. Instance path: {instance.Location}");
        }

        options.ValidationPathStack.PushSchemaResource(referencedSchemaResource);
        options.ValidationPathStack.PushReferencedSchema(referencedSchemaResource, FullUriRef);

        ValidationResult validationResult = referencedSchema.ValidateCore(instance, options);

        options.ValidationPathStack.PopReferencedSchema();
        options.ValidationPathStack.PopSchemaResource();
        options.SchemaRecursionRecorder.PopRecord();

        return validationResult;
    }
}