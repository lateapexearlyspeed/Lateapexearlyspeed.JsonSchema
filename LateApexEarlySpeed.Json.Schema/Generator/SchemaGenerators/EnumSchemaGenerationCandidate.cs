using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class EnumSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        var typeKeyword = new TypeKeyword(InstanceType.String);

        IEnumerable<JsonInstanceElement> allowedStringEnums = typeToConvert.GetEnumNames().Select(name => new JsonInstanceElement(JsonSerializer.SerializeToElement(name), ImmutableJsonPointer.Empty));
        var enumKeyword = new EnumKeyword(allowedStringEnums.ToList());

        var keywords = new List<KeywordBase> { typeKeyword, enumKeyword };
        keywords.AddRange(keywordsFromProperty);
        keywords.AddRange(SchemaGenerationHelper.GenerateKeywordsFromType(typeToConvert));

        return new BodyJsonSchema(keywords);
    }
}