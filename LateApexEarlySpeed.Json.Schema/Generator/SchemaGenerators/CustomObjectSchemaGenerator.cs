using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class CustomObjectSchemaGenerator : ISchemaGenerator
{
    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.Null);

        IEnumerable<KeywordBase> keywordsOnType = SchemaGenerationHelper.GenerateKeywordsFromType(typeToConvert);

        PropertyInfo[] propertyInfos = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

        PropertiesKeyword propertiesKeyword = CreatePropertiesKeyword(propertyInfos, options);

        RequiredKeyword? requiredKeyword = CreateRequiredKeyword(propertyInfos, options);

        IEnumerable<KeywordBase> keywords = keywordsOnType.Append(typeKeyword).Append(propertiesKeyword);
        if (requiredKeyword is not null)
        {
            keywords = keywords.Append(requiredKeyword);
        }

        return new JsonSchemaResource(new Uri(typeToConvert.FullName!, UriKind.Relative), keywords.ToList(), new List<ISchemaContainerValidationNode>(0), null, null, null, null, null);
    }

    private static PropertiesKeyword CreatePropertiesKeyword(PropertyInfo[] propertyInfos, JsonSchemaGeneratorOptions options)
    {
        var propertiesSchemas = new Dictionary<string, JsonSchema>();

        foreach (PropertyInfo propertyInfo in propertyInfos.Where(prop => prop.GetCustomAttribute<JsonIgnoreAttribute>() is null))
        {
            KeywordBase[] keywordsOfProp = GenerateKeywordsFromPropertyInfo(propertyInfo);
            JsonSchema propertySchema = JsonSchemaGenerator.GenerateSchema(propertyInfo.PropertyType, keywordsOfProp, options);

            if (propertySchema is JsonSchemaResource propertySchemaResource)
            {
                options.SchemaDefinitions.AddSchemaDefinition(propertyInfo.PropertyType, propertySchemaResource);

                propertySchema = SchemaGenerationHelper.GenerateSchemaReference(propertyInfo.PropertyType, keywordsOfProp);
            }

            if (propertyInfo.GetCustomAttribute<NotNullAttribute>() is not null)
            {
                var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array);
                var allOfKeyword = new AllOfKeyword(new List<JsonSchema> { propertySchema, new BodyJsonSchema(new List<KeywordBase> { typeKeyword }) });

                propertySchema = new BodyJsonSchema(new List<KeywordBase> { allOfKeyword });
            }

            propertiesSchemas[GetPropertyName(propertyInfo, options)] = propertySchema;
        }

        return new PropertiesKeyword(propertiesSchemas);
    }

    private static string GetPropertyName(PropertyInfo propertyInfo, JsonSchemaGeneratorOptions options)
    {
        JsonPropertyNameAttribute? jsonPropertyNameAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();

        return jsonPropertyNameAttribute is null
            ? options.PropertyNamingPolicy.ConvertName(propertyInfo.Name)
            : jsonPropertyNameAttribute.Name;
    }

    private static RequiredKeyword? CreateRequiredKeyword(PropertyInfo[] properties, JsonSchemaGeneratorOptions options)
    {
        string[] requiredPropertyNames = properties
            .Where(prop => prop.GetCustomAttribute<JsonRequiredAttribute>() is not null || prop.GetCustomAttribute<RequiredAttribute>() is not null)
            .Select(propertyInfo => GetPropertyName(propertyInfo, options)).ToArray();
        return requiredPropertyNames.Length == 0
            ? null
            : new RequiredKeyword(requiredPropertyNames);
    }

    /// <summary>
    /// Extract attributes from <see cref="PropertyInfo"/>
    /// </summary>
    private static KeywordBase[] GenerateKeywordsFromPropertyInfo(PropertyInfo propertyInfo)
    {
        IEnumerable<IKeywordGenerator> keywordGeneratorOnType = propertyInfo.GetCustomAttributes().OfType<IKeywordGenerator>();
        return keywordGeneratorOnType.Select(keywordGenerator => keywordGenerator.CreateKeyword(propertyInfo.PropertyType)).ToArray();
    }
}