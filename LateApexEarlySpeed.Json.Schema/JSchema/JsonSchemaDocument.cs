using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal static class JsonSchemaDocument
{
    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(ReadOnlySpan<char> schema, SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions options)
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSchemaDeserializerContext(options.PropertyNameCaseInsensitive, options.DefaultDialect).ToJsonSerializerOptions();

        IJsonSchemaDocument doc = JsonSerializer.Deserialize<IJsonSchemaDocument>(schema, jsonSerializerOptions)!;

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            if (options.IgnoreResourceIdInUnknownKeyword)
            {
                bodyDoc.RemoveIdFromAllInvalidKeywordPropertiesRecursively();
            }

            UpdateDocWithGlobalResourceRegistry(bodyDoc, globalSchemaResourceRegistry);
        }

        return doc;
    }

    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(Stream utf8Schema, SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions options)
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSchemaDeserializerContext(options.PropertyNameCaseInsensitive, options.DefaultDialect).ToJsonSerializerOptions();

        IJsonSchemaDocument doc = JsonSerializer.Deserialize<IJsonSchemaDocument>(utf8Schema, jsonSerializerOptions)!;

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            if (options.IgnoreResourceIdInUnknownKeyword)
            {
                bodyDoc.RemoveIdFromAllInvalidKeywordPropertiesRecursively();
            }

            UpdateDocWithGlobalResourceRegistry(bodyDoc, globalSchemaResourceRegistry);
        }

        return doc;
    }

    public static void UpdateDocWithGlobalResourceRegistry(BodyJsonSchemaDocument schemaDoc, SchemaResourceRegistry globalSchemaResourceRegistry)
    {
        schemaDoc.MakeAllIdentifierAndReferenceBeFullUri();
        globalSchemaResourceRegistry.AddSchemaResourcesFromRegistry(schemaDoc.LocalSchemaResourceRegistry);
        schemaDoc.GlobalSchemaResourceRegistry = globalSchemaResourceRegistry;
    }
}