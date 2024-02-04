using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class ArbitraryJsonTypesSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(JsonElement) || typeToConvert == typeof(JsonDocument) || typeToConvert == typeof(JsonNode) || typeToConvert == typeof(JsonValue);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        return new BodyJsonSchema(keywordsFromProperty.ToList());
    }
}