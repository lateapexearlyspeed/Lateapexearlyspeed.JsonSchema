using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class NullableValueTypeSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return Nullable.GetUnderlyingType(typeToConvert) is not null;
    }

    public BodyJsonSchema Generate(IType typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        Debug.Assert(typeToConvert.GenericTypeArguments.Length == 1);

        IType underlyingType = typeToConvert.GenericTypeArguments[0];

        BodyJsonSchema underlyingSchema = JsonSchemaGenerator.GenerateSchema(underlyingType, keywordsFromProperty, options);

        var nullTypeSchema = new BodyJsonSchema(new KeywordBase[] { new TypeKeyword(InstanceType.Null) });

        if (underlyingSchema is JsonSchemaResource schemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(underlyingType.Type, schemaResource);
            underlyingSchema = SchemaGenerationHelper.GenerateSchemaReference(underlyingType.Type, keywordsFromProperty, options.MainDocumentBaseUri!);
        }

        var anyOfKeyword = new AnyOfKeyword(new [] { nullTypeSchema, underlyingSchema });

        return new BodyJsonSchema(new KeywordBase[] { anyOfKeyword });
    }
}