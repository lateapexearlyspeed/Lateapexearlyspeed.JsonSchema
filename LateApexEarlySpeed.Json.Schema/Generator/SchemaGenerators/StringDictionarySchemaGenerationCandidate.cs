using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class StringDictionarySchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>) && typeToConvert.GetGenericArguments()[0] == typeof(string);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.Null);
        Type valueType = typeToConvert.GetGenericArguments()[1];
        JsonSchema valueSchema = JsonSchemaGenerator.GenerateSchema(valueType, Enumerable.Empty<KeywordBase>(), options);

        JsonSchema propertySchema;
        if (valueSchema is JsonSchemaResource valueSchemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(valueType, valueSchemaResource);

            propertySchema = SchemaGenerationHelper.GenerateSchemaReference(valueType, Enumerable.Empty<KeywordBase>());
        }
        else
        {
            propertySchema = valueSchema;
        }

        var additionalPropertiesKeyword = new AdditionalPropertiesKeyword
        {
            Schema = propertySchema
        };

        var keywords = new List<KeywordBase> { typeKeyword, additionalPropertiesKeyword };
        keywords.AddRange(keywordsFromProperty);

        return new BodyJsonSchema(keywords);
    }
}