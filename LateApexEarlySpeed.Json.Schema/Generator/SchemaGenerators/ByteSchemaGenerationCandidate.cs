using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class ByteSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(byte);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        return SchemaGenerationHelper.GenerateSchemaForUnsignedInteger(keywordsFromProperty, byte.MaxValue);
    }
}