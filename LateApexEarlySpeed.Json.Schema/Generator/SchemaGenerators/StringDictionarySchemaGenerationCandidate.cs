using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class StringDictionarySchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Dictionary<,>) && typeToConvert.GetGenericArguments()[0] == typeof(string);
    }

    public BodyJsonSchema Generate(IType typeToConvert, IEnumerable<ValidationKeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.Null);
        IType valueType = typeToConvert.GenericTypeArguments[1];
        JsonSchema valueSchema = JsonSchemaGenerator.GenerateSchema(valueType, Enumerable.Empty<ValidationKeywordBase>(), options);

        JsonSchema propertySchema;
        if (valueSchema is JsonSchemaResource valueSchemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(valueType.Type, valueSchemaResource);

            propertySchema = SchemaGenerationHelper.GenerateSchemaReference(valueType.Type, Enumerable.Empty<ValidationKeywordBase>(), options.MainDocumentBaseUri!);
        }
        else
        {
            propertySchema = valueSchema;
        }

        var additionalPropertiesKeyword = new AdditionalPropertiesKeyword
        {
            Schema = propertySchema
        };

        var keywords = new List<ValidationKeywordBase> { typeKeyword, additionalPropertiesKeyword };
        keywords.AddRange(keywordsFromProperty);

        return new BodyJsonSchema(keywords);
    }
}