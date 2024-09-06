using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class CollectionSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert.GetInterface("IEnumerable`1") is not null;
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        List<KeywordBase> keywords = new List<KeywordBase> { new TypeKeyword(InstanceType.Array, InstanceType.Null) };
        keywords.AddRange(keywordsFromProperty);

        Type? enumerableInterface = typeToConvert.GetInterface("IEnumerable`1");
        Debug.Assert(enumerableInterface is not null);
        Type elementType = enumerableInterface.GetGenericArguments()[0];
        JsonSchema elementSchema = JsonSchemaGenerator.GenerateSchema(elementType, Enumerable.Empty<KeywordBase>(), options);

        JsonSchema itemsSchema;
        if (elementSchema is JsonSchemaResource elementSchemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(elementType, elementSchemaResource);

            itemsSchema = SchemaGenerationHelper.GenerateSchemaReference(elementType, Enumerable.Empty<KeywordBase>(), options.MainDocumentBaseUri!);
        }
        else
        {
            itemsSchema = elementSchema;
        }

        var itemsKeyword = new ItemsKeyword { Schema = itemsSchema };
        keywords.Add(itemsKeyword);

        return new BodyJsonSchema(keywords);
    }
}