using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class NullableValueTypeSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        Debug.Assert(typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>));

        Type underlyingType = Nullable.GetUnderlyingType(typeToConvert)!;

        BodyJsonSchema underlyingSchema = JsonSchemaGenerator.GenerateSchema(underlyingType, keywordsFromProperty, options);

        var nullTypeSchema = new BodyJsonSchema(new KeywordBase[] { new TypeKeyword(InstanceType.Null) });

        if (underlyingSchema is JsonSchemaResource schemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(underlyingType, schemaResource);
            underlyingSchema = SchemaGenerationHelper.GenerateSchemaReference(underlyingType, keywordsFromProperty, options.MainDocumentBaseUri!);
        }

        var anyOfKeyword = new AnyOfKeyword(new [] { nullTypeSchema, underlyingSchema });

        return new BodyJsonSchema(new KeywordBase[] { anyOfKeyword });
    }
}