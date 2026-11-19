using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal static class JsonSchemaDocument
{
    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(ReadOnlySpan<char> schema, SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions options, Uri? retrievalUri)
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSchemaDeserializerContext(options, options.DefaultDialect).ToJsonSerializerOptions();

        IJsonSchemaDocument doc = JsonSerializer.Deserialize<IJsonSchemaDocument>(schema, jsonSerializerOptions)!;

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            if (options.IgnoreResourceIdInUnknownKeyword)
            {
                bodyDoc.RemoveIdFromAllInvalidKeywordPropertiesRecursively();
            }

            UpdateDocWithGlobalResourceRegistry(bodyDoc, globalSchemaResourceRegistry, retrievalUri);
        }

        return doc;
    }

    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(Stream utf8Schema, SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions options, Uri? retrievalUri)
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSchemaDeserializerContext(options, options.DefaultDialect).ToJsonSerializerOptions();

        IJsonSchemaDocument doc = JsonSerializer.Deserialize<IJsonSchemaDocument>(utf8Schema, jsonSerializerOptions)!;

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            if (options.IgnoreResourceIdInUnknownKeyword)
            {
                bodyDoc.RemoveIdFromAllInvalidKeywordPropertiesRecursively();
            }

            UpdateDocWithGlobalResourceRegistry(bodyDoc, globalSchemaResourceRegistry, retrievalUri);
        }

        return doc;
    }

    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(JsonElement schema, SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions options, Uri? retrievalUri)
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSchemaDeserializerContext(options, options.DefaultDialect).ToJsonSerializerOptions();

        IJsonSchemaDocument doc = schema.Deserialize<IJsonSchemaDocument>(jsonSerializerOptions)!;

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            if (options.IgnoreResourceIdInUnknownKeyword)
            {
                bodyDoc.RemoveIdFromAllInvalidKeywordPropertiesRecursively();
            }

            UpdateDocWithGlobalResourceRegistry(bodyDoc, globalSchemaResourceRegistry, retrievalUri);
        }

        return doc;
    }

    public static void UpdateDocWithGlobalResourceRegistry(BodyJsonSchemaDocument schemaDoc, SchemaResourceRegistry globalSchemaResourceRegistry, Uri? retrievalUri)
    {
        schemaDoc.MakeAllIdentifierAndReferenceBeFullUri(retrievalUri ?? BodyJsonSchemaDocument.DefaultDocumentBaseUri);

        if (retrievalUri is not null && retrievalUri != schemaDoc.BaseUri)
        {
            schemaDoc.LocalSchemaResourceRegistry.AddSchemaResource(retrievalUri, schemaDoc);
        }

        globalSchemaResourceRegistry.AddSchemaResourcesFromRegistry(schemaDoc.LocalSchemaResourceRegistry);

        schemaDoc.GlobalSchemaResourceRegistry = globalSchemaResourceRegistry;
    }
}