using System.Reflection;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

internal class JsonSchemaGeneratorOptions
{
    public Dictionary<string, JsonSchemaResource> SchemaResourceDefinitions = new();
}

public class JsonSchemaGenerator
{
    public IJsonSchemaDocument GenerateJsonSchemaDocument<T>()
    {
        JsonSchemaGeneratorOptions options = new JsonSchemaGeneratorOptions();
        BodyJsonSchema jsonSchema = GenerateSchema(typeof(T), options, Enumerable.Empty<KeywordBase>());

        return jsonSchema.TransformToSchemaDocument(new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName), new DefsKeyword(options.SchemaResourceDefinitions.ToDictionary(kv => kv.Key, kv => kv.Value as JsonSchema)));
    }

    private static BodyJsonSchema GenerateSchema(Type type, JsonSchemaGeneratorOptions options, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        if (options.SchemaResourceDefinitions.TryGetValue(type.FullName!, out JsonSchemaResource? schemaDefinition))
        {
            return schemaDefinition;
        }

        // Integer
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(sbyte)
            || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(byte))
        {
            return GenerateSchemaForInteger(keywordsFromProperty);
        }

        // Double
        if (type == typeof(float) || type == typeof(double))
        {
            return GenerateSchemaForDouble(keywordsFromProperty);
        }

        // Boolean
        if (type == typeof(bool))
        {
            return GenerateSchemaForBoolean(keywordsFromProperty);
        }

        // String
        if (type == typeof(string))
        {
            return GenerateSchemaForString(keywordsFromProperty);
        }

        // Array
        if (type.IsArray)
        {
            return GenerateSchemaForArray(type, options, keywordsFromProperty);
        }

        // Dictionary<string, TValue>
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) && type.GetGenericArguments()[0] == typeof(string))
        {
            return GenerateSchemaForStringDictionary(type, options, keywordsFromProperty);
        }

        // Enum
        if (type.IsEnum)
        {
            return GenerateSchemaForEnum(type, keywordsFromProperty);
        }

        // Custom object
        return GenerateSchemaForCustomObject(type, options, keywordsFromProperty);
    }

    private static BodyJsonSchema GenerateSchemaForCustomObject(Type type, JsonSchemaGeneratorOptions options, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword { InstanceTypes = new[] { InstanceType.Object } };

        IEnumerable<KeywordBase> keywordsOnType = GenerateKeywordsFromMemberInfoAttributes(type);

        PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        var propertiesKeyword = new PropertiesKeyword
        {
            PropertiesSchemas = propertyInfos.ToDictionary(prop => prop.Name, prop =>
            {
                JsonSchema propertySchema = GenerateSchema(prop.PropertyType, options, GenerateKeywordsFromMemberInfoAttributes(prop));

                if (propertySchema is JsonSchemaResource propertySchemaResource)
                {
                    options.SchemaResourceDefinitions.TryAdd(prop.PropertyType.FullName!, propertySchemaResource);
                    return GenerateSchemaReference(prop.PropertyType);
                }
                else
                {
                    return propertySchema;
                }
            })
        };

        return new JsonSchemaResource(new Uri(type.FullName!, UriKind.Relative), keywordsFromProperty.Concat(keywordsOnType).Append(typeKeyword).Append(propertiesKeyword).ToList(), new List<ISchemaContainerValidationNode>(0), null, null, null, null, null);
    }

    private static BodyJsonSchema GenerateSchemaForEnum(Type type, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword { InstanceTypes = new[] { InstanceType.String } };

        IEnumerable<JsonInstanceElement> allowedStringEnums = type.GetEnumNames().Select(name => new JsonInstanceElement(JsonSerializer.SerializeToElement(name), ImmutableJsonPointer.Empty));
        var enumKeyword = new EnumKeyword(allowedStringEnums.ToList());

        var keywords = new List<KeywordBase> { typeKeyword, enumKeyword };
        keywords.AddRange(keywordsFromProperty);
        keywords.AddRange(GenerateKeywordsFromMemberInfoAttributes(type));

        return new BodyJsonSchema(keywords);
    }

    /// <summary>
    /// Extract attributes from either header of <see cref="Type"/> itself or <see cref="PropertyInfo"/>
    /// </summary>
    private static IEnumerable<KeywordBase> GenerateKeywordsFromMemberInfoAttributes(MemberInfo memberInfo)
    {
        IEnumerable<IKeywordGenerator> keywordGeneratorOnType = memberInfo.GetCustomAttributes().OfType<IKeywordGenerator>();
        return keywordGeneratorOnType.Select(keywordGenerator => keywordGenerator.CreateKeyword());
    }

    private static BodyJsonSchema GenerateSchemaForStringDictionary(Type type, JsonSchemaGeneratorOptions options, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword { InstanceTypes = new[] { InstanceType.Object } };
        Type valueType = type.GetGenericArguments()[1];
        JsonSchema valueSchema = GenerateSchema(valueType, options, Enumerable.Empty<KeywordBase>());

        JsonSchema propertySchema;
        if (valueSchema is JsonSchemaResource valueSchemaResource)
        {
            options.SchemaResourceDefinitions.TryAdd(valueType.FullName!, valueSchemaResource);
            propertySchema = GenerateSchemaReference(valueType);
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

    private static BodyJsonSchema GenerateSchemaForArray(Type type, JsonSchemaGeneratorOptions options, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        List<KeywordBase> keywords = new List<KeywordBase> { new TypeKeyword { InstanceTypes = new[] { InstanceType.Array } } };
        keywords.AddRange(keywordsFromProperty);
        
        Type elementType = type.GetElementType()!;
        JsonSchema elementSchema = GenerateSchema(elementType, options, Enumerable.Empty<KeywordBase>());

        JsonSchema itemsSchema;
        if (elementSchema is JsonSchemaResource elementSchemaResource)
        {
            options.SchemaResourceDefinitions.TryAdd(elementType.FullName!, elementSchemaResource);
            itemsSchema = GenerateSchemaReference(elementType);
        }
        else
        {
            itemsSchema = elementSchema;
        }

        var itemsKeyword = new ItemsKeyword { Schema = itemsSchema };
        keywords.Add(itemsKeyword);

        return new BodyJsonSchema(keywords);
    }

    private static BodyJsonSchema GenerateSchemaForString(IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword { InstanceTypes = new[] { InstanceType.String } }).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForBoolean(IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword { InstanceTypes = new[] { InstanceType.Boolean } }).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForDouble(IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword { InstanceTypes = new[] { InstanceType.Number } }).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForInteger(IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword { InstanceTypes = new[] { InstanceType.Integer } }).ToList());
    }

    private static BodyJsonSchema GenerateSchemaReference(Type type)
    {
        return new BodyJsonSchema(new List<KeywordBase>(0), new List<ISchemaContainerValidationNode>(0), new SchemaReferenceKeyword(new Uri(type.FullName!, UriKind.Relative)), null, null, null, null);
    }
}
