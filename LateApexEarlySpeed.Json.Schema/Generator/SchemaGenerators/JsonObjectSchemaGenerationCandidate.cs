using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json.Nodes;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class JsonObjectSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(JsonObject);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        return SchemaGenerationHelper.GenerateSchemaForJsonType(InstanceType.Object, keywordsFromProperty);
    }
}