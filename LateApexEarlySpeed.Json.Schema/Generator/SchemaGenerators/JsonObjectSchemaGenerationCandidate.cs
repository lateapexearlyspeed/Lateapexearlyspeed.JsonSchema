using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json.Nodes;
using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class JsonObjectSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(JsonObject);
    }

    public BodyJsonSchema Generate(IType typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        return SchemaGenerationHelper.GenerateSchemaForJsonType(InstanceType.Object, keywordsFromProperty);
    }
}