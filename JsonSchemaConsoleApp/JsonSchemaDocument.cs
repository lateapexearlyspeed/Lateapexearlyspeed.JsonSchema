using System.Text.Json;

namespace JsonSchemaConsoleApp;

internal static class JsonSchemaDocument
{
    public static IJsonSchemaDocument Create(string schema, SchemaResourceRegistry globalSchemaResourceRegistry)
    {
        IJsonSchemaDocument doc = JsonSerializer.Deserialize<IJsonSchemaDocument>(schema);

        if (doc is BodyJsonSchemaDocument bodyDoc)
        {
            bodyDoc.MakeAllIdentifierAndReferenceBeFullUri();
            globalSchemaResourceRegistry.Add(bodyDoc.LocalSchemaResourceRegistry);

            bodyDoc.GlobalSchemaResourceRegistry = globalSchemaResourceRegistry;
        }

        return doc;
    }
}