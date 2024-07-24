using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal static class JsonSchemaDocument
{
    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(ReadOnlySpan<char> schema, SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions options)
    {
        JsonSerializerOptions jsonSerializerOptions = JsonValidatorOptionsJsonSerializerOptionsMapper.ToJsonSerializerOptions(options);

        IJsonSchemaDocument doc = JsonSerializer.Deserialize<IJsonSchemaDocument>(schema, jsonSerializerOptions)!;

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            bodyDoc.MakeAllIdentifierAndReferenceBeFullUri();
            globalSchemaResourceRegistry.AddSchemaResourcesFromRegistry(bodyDoc.LocalSchemaResourceRegistry);

            bodyDoc.GlobalSchemaResourceRegistry = globalSchemaResourceRegistry;
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