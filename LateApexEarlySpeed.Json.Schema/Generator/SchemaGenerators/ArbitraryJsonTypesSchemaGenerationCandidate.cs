using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json.Nodes;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class ArbitraryJsonTypesSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(JsonElement) || typeToConvert == typeof(JsonDocument) || typeToConvert == typeof(JsonNode) || typeToConvert == typeof(JsonValue);
    }

    public BodyJsonSchema Generate(IType typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        return new BodyJsonSchema(keywordsFromProperty);
    }
}