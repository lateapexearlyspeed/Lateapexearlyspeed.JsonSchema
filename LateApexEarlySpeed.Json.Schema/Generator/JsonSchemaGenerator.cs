using LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class JsonSchemaGeneratorOptions
{
    internal TypeSchemaDefinitions SchemaDefinitions = new();
    public JsonSchemaNamingPolicy PropertyNamingPolicy { get; set; } = JsonSchemaNamingPolicy.SharedDefault;
}

public static class JsonSchemaGenerator
{
    public static JsonValidator GenerateJsonValidator<T>(JsonSchemaGeneratorOptions? options = null) => GenerateJsonValidator(typeof(T), options);

    public static JsonValidator GenerateJsonValidator(Type type, JsonSchemaGeneratorOptions? options = null)
    {
        options ??= new JsonSchemaGeneratorOptions();
        BodyJsonSchema jsonSchema = GenerateSchema(type, Enumerable.Empty<KeywordBase>(), options);

        BodyJsonSchemaDocument bodyJsonSchemaDocument = jsonSchema.TransformToSchemaDocument(new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, "[Main]-" + type.FullName), new DefsKeyword(options.SchemaDefinitions.GetAll().ToDictionary(kv => kv.Key, kv => kv.Value as JsonSchema)));

        return new JsonValidator(bodyJsonSchemaDocument);
    }

    internal static BodyJsonSchema GenerateSchema(Type type, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        JsonSchemaResource? schemaDefinition = options.SchemaDefinitions.GetSchemaDefinition(type);

        if (schemaDefinition is not null)
        {
            return schemaDefinition;
        }

        ISchemaGenerator schemaGenerator = SchemaGeneratorSelector.Select(type);
        return schemaGenerator.Generate(type, keywordsFromProperty, options);
    }
}
