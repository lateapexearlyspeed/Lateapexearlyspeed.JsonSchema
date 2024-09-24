using LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class JsonSchemaGeneratorOptions
{
    /// <summary>
    /// User entry point to create <see cref="JsonSchemaGeneratorOptions"/> instance
    /// </summary>
    public JsonSchemaGeneratorOptions()
    {
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
            IgnoreNullableReferenceTypeAnnotation = options.IgnoreNullableReferenceTypeAnnotation;
        }
    }

    internal TypeSchemaDefinitions SchemaDefinitions { get; } = new();
    internal Uri? MainDocumentBaseUri { get; set; }
    internal TypeGenerationRecorder TypeGenerationRecorder { get; } = new();

    public JsonSchemaNamingPolicy PropertyNamingPolicy { get; set; } = JsonSchemaNamingPolicy.SharedDefault;
    public bool IgnoreNullableReferenceTypeAnnotation { get; set; }
}

public static class JsonSchemaGenerator
{
    public static JsonValidator GenerateJsonValidator<T>(JsonSchemaGeneratorOptions? options = null) => GenerateJsonValidator(typeof(T), options);

    public static JsonValidator GenerateJsonValidator(Type type, JsonSchemaGeneratorOptions? options = null)
    {
        var mainDocumentBaseUri = new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, "[Main]-" + type.FullName);
        options = new JsonSchemaGeneratorOptions(options, mainDocumentBaseUri);

        BodyJsonSchema jsonSchema = GenerateSchema(type, Enumerable.Empty<KeywordBase>(), options);

        BodyJsonSchemaDocument bodyJsonSchemaDocument = jsonSchema.TransformToSchemaDocument(mainDocumentBaseUri, new DefsKeyword(options.SchemaDefinitions.GetAll().ToDictionary(kv => kv.Key, kv => kv.Value as JsonSchema)));

        return new JsonValidator(bodyJsonSchemaDocument);
    }

    internal static BodyJsonSchema GenerateSchema(Type type, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        JsonSchemaResource? schemaDefinition = options.SchemaDefinitions.GetSchemaDefinition(type);

        if (schemaDefinition is not null)
        {
            return schemaDefinition;
        }

        options.TypeGenerationRecorder.PushType(type);
        
        ISchemaGenerator schemaGenerator = SchemaGeneratorSelector.Select(type);
        BodyJsonSchema bodyJsonSchema = schemaGenerator.Generate(type, keywordsFromProperty, options);
        
        options.TypeGenerationRecorder.PopType();

        return bodyJsonSchema;
    }
}
