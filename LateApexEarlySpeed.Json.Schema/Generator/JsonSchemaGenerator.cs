using LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;
using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class JsonSchemaGeneratorOptions
{
    /// <summary>
    /// User entry point to create <see cref="JsonSchemaGeneratorOptions"/> instance
    /// </summary>
    public JsonSchemaGeneratorOptions()
    {
        NullabilityTypeInfo = new NullabilityTypeInfo();
    }

    /// <summary>
    /// Copy user defined <paramref name="options"/> and set <see cref="MainDocumentBaseUri"/>
    /// </summary>
    internal JsonSchemaGeneratorOptions(JsonSchemaGeneratorOptions? options, Uri mainDocumentBaseUri)
    {
        MainDocumentBaseUri = mainDocumentBaseUri;

        if (options is not null)
        {
            PropertyNamingPolicy = options.PropertyNamingPolicy;
            NullabilityTypeInfo = options.NullabilityTypeInfo;
        }
        else
        {
            NullabilityTypeInfo = new NullabilityTypeInfo();
        }
    }

    internal TypeSchemaDefinitions SchemaDefinitions { get; } = new();
    internal Uri? MainDocumentBaseUri { get; set; }
    internal TypeGenerationRecorder TypeGenerationRecorder { get; } = new();
    public JsonSchemaNamingPolicy PropertyNamingPolicy { get; set; } = JsonSchemaNamingPolicy.SharedDefault;
    public NullabilityTypeInfo NullabilityTypeInfo { get; }
}

public static class JsonSchemaGenerator
{
    public static JsonValidator GenerateJsonValidator<T>(JsonSchemaGeneratorOptions? options = null) => GenerateJsonValidator(typeof(T), options);

    public static JsonValidator GenerateJsonValidator(Type type, JsonSchemaGeneratorOptions? options = null)
    {
        var mainDocumentBaseUri = new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, "[Main]-" + type.FullName);
        options = new JsonSchemaGeneratorOptions(options, mainDocumentBaseUri);

        IType abstractType;
        NullabilityTypeInfo nullabilityTypeInfo = options.NullabilityTypeInfo;

        if (nullabilityTypeInfo.ReferenceTypeNullabilityPolicy.UseNullabilityAnnotation)
        {
            NullabilityType nullabilityType;

            if (nullabilityTypeInfo.ArrayElementNullability is not null)
            {
                nullabilityType = NullabilityType.GetType(type, nullabilityTypeInfo.ArrayElementNullability);
            }
            else if (nullabilityTypeInfo.GenericTypeArgumentsNullabilities is not null && nullabilityTypeInfo.GenericTypeArgumentsNullabilities.Length != 0)
            {
                nullabilityType = NullabilityType.GetType(type, nullabilityTypeInfo.GenericTypeArgumentsNullabilities);
            }
            else
            {
                nullabilityType = NullabilityType.GetType(type);
            }

            abstractType = new NullabilityTypeWrapper(nullabilityType);
        }
        else
        {
            abstractType = new TypeWrapper(type);
        }

        BodyJsonSchema jsonSchema = GenerateSchema(abstractType, Enumerable.Empty<KeywordBase>(), options);

        BodyJsonSchemaDocument bodyJsonSchemaDocument = jsonSchema.TransformToSchemaDocument(mainDocumentBaseUri, new DefsKeyword(options.SchemaDefinitions.GetAll().ToDictionary(kv => kv.Key, kv => kv.Value as JsonSchema)));

        return new JsonValidator(bodyJsonSchemaDocument);
    }

    internal static BodyJsonSchema GenerateSchema(IType type, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        JsonSchemaResource? schemaDefinition = options.SchemaDefinitions.GetSchemaDefinition(type.Type);

        if (schemaDefinition is not null)
        {
            return schemaDefinition;
        }

        options.TypeGenerationRecorder.PushType(type.Type);
        
        ISchemaGenerator schemaGenerator = SchemaGeneratorSelector.Select(type.Type);
        BodyJsonSchema bodyJsonSchema = schemaGenerator.Generate(type, keywordsFromProperty, options);
        
        options.TypeGenerationRecorder.PopType();

        return bodyJsonSchema;
    }
}
