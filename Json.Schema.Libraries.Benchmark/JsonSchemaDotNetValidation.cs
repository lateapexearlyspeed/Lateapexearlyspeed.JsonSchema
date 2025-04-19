using System.Text.Json.Nodes;

namespace Json.Schema.Libraries.Benchmark;

internal class JsonSchemaDotNetValidation : IJsonSchemaValidation
{
    public JsonSchemaLibraryKinds LibraryKinds => JsonSchemaLibraryKinds.JsonSchemaDotNet;

    public bool Validate(string jsonSchema, string instance)
    {
        JsonSchema schema = JsonSchema.FromText(jsonSchema);

        return schema.Evaluate(JsonNode.Parse(instance)).IsValid;
    }
}