using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal static class JsonSchemaDocument
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions;

    static JsonSchemaDocument()
    {
        DefaultJsonSerializerOptions = new JsonSerializerOptions();
        DefaultJsonSerializerOptions.AddJsonValidatorOptions(JsonValidatorOptions.Default);
    }

    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(ReadOnlySpan<char> schema, SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions options)
    {
        JsonSerializerOptions jsonSerializerOptions;

        // Try to reuse DefaultJsonSerializerOptions
        if (options.Equals(JsonValidatorOptions.Default))
        {
            jsonSerializerOptions = DefaultJsonSerializerOptions;
        }
        else
        {
            jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.AddJsonValidatorOptions(options);
        }

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