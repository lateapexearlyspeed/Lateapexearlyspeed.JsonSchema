using System;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal static class JsonSchemaDocument
{
    public static IJsonSchemaDocument CreateDocAndUpdateGlobalResourceRegistry(ReadOnlySpan<char> schema, SchemaResourceRegistry globalSchemaResourceRegistry)
    {
        IJsonSchemaDocument doc = JsonSerializer.Deserialize<IJsonSchemaDocument>(schema)!;

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            bodyDoc.MakeAllIdentifierAndReferenceBeFullUri();
            globalSchemaResourceRegistry.AddSchemaResourcesFromRegistry(bodyDoc.LocalSchemaResourceRegistry);

            bodyDoc.GlobalSchemaResourceRegistry = globalSchemaResourceRegistry;
        }

        return doc;
    }
}