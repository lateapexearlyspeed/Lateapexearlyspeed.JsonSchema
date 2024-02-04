using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class DateTimeSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert == typeof(DateTime);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        var typeKeyword = new TypeKeyword(InstanceType.String);
        var formatKeyword = new FormatKeyword(DotNetDateTimeFormatValidator.FormatName);

        return new BodyJsonSchema(new List<KeywordBase>(keywordsFromProperty) { typeKeyword, formatKeyword });
    }
}