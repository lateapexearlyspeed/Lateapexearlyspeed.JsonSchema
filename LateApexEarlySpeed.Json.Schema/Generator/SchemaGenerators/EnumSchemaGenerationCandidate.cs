using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class EnumSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        IEnumerable<JsonInstanceElement> allowedStringEnums = typeToConvert.GetEnumNames().Select(name => JsonInstanceSerializer.SerializeToElement(name));
        IEnumerable<JsonInstanceElement> allowedNumberEnums = typeToConvert.GetEnumValues().Select(JsonInstanceSerializer.SerializeToElement);
        var enumKeyword = new EnumKeyword(allowedStringEnums.Concat(allowedNumberEnums).ToList());

        var keywords = new List<KeywordBase> { enumKeyword };
        keywords.AddRange(keywordsFromProperty);
        keywords.AddRange(SchemaGenerationHelper.GenerateKeywordsFromType(typeToConvert));

        return new BodyJsonSchema(keywords);
    }
}