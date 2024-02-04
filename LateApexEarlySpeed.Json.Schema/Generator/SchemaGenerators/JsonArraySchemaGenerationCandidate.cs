using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json.Nodes;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class JsonArraySchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(JsonArray);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        return SchemaGenerationHelper.GenerateSchemaForJsonType(InstanceType.Array, keywordsFromProperty);
    }
}