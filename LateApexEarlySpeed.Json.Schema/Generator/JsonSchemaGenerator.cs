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
    public TypeSchemaDefinitions SchemaDefinitions = new();
}

internal class TypeSchemaDefinitions
{
    private readonly Dictionary<string, JsonSchemaResource> _schemaResourceDefinitions = new();

    public JsonSchemaResource? GetSchemaDefinition(Type type)
    {
        return _schemaResourceDefinitions.GetValueOrDefault(GetDefName(type));
    }

    public void AddSchemaDefinition(Type type, JsonSchemaResource schemaResource)
    {
        _schemaResourceDefinitions.TryAdd(GetDefName(type), schemaResource);
    }

    public Dictionary<string, JsonSchemaResource> GetAll()
    {
        return _schemaResourceDefinitions;
    }

    public static string GetDefName(Type type)
    {
        return type.FullName!;
    }
}

public class JsonSchemaGenerator
{
    public IJsonSchemaDocument GenerateJsonSchemaDocument<T>()
    {
        JsonSchemaGeneratorOptions options = new JsonSchemaGeneratorOptions();
        BodyJsonSchema jsonSchema = GenerateSchema(typeof(T), options, Array.Empty<KeywordBase>());

        return jsonSchema.TransformToSchemaDocument(new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, typeof(T).FullName), new DefsKeyword(options.SchemaDefinitions.GetAll().ToDictionary(kv => kv.Key, kv => kv.Value as JsonSchema)));
    }

    private static BodyJsonSchema GenerateSchema(Type type, JsonSchemaGeneratorOptions options, KeywordBase[] keywordsFromProperty)
    {
        JsonSchemaResource? schemaDefinition = options.SchemaDefinitions.GetSchemaDefinition(type);

        if (schemaDefinition is not null)
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
        return GenerateSchemaForCustomObject(type, options);
    }

    private static BodyJsonSchema GenerateSchemaForCustomObject(Type type, JsonSchemaGeneratorOptions options)
    {
        var typeKeyword = new TypeKeyword { InstanceTypes = new[] { InstanceType.Object } };

        IEnumerable<KeywordBase> keywordsOnType = GenerateKeywordsFromType(type);

        PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        var propertiesKeyword = new PropertiesKeyword
        {
            PropertiesSchemas = propertyInfos.ToDictionary(prop => prop.Name, prop =>
            {
                KeywordBase[] keywordsOfProp = GenerateKeywordsFromPropertyInfo(prop);
                JsonSchema propertySchema = GenerateSchema(prop.PropertyType, options, keywordsOfProp);

                if (propertySchema is JsonSchemaResource propertySchemaResource)
                {
                    options.SchemaDefinitions.AddSchemaDefinition(prop.PropertyType, propertySchemaResource);

                    return GenerateSchemaReference(prop.PropertyType, keywordsOfProp);
                }
                else
                {
                    return propertySchema;
                }
            })
        };

        RequiredKeyword? requiredKeyword = CreateRequiredKeyword(propertyInfos);

        IEnumerable<KeywordBase> keywords = keywordsOnType.Append(typeKeyword).Append(propertiesKeyword);
        if (requiredKeyword is not null)
        {
            keywords = keywords.Append(requiredKeyword);
        }
        return new JsonSchemaResource(new Uri(type.FullName!, UriKind.Relative), keywords.ToList(), new List<ISchemaContainerValidationNode>(0), null, null, null, null, null);
    }

    private static RequiredKeyword? CreateRequiredKeyword(PropertyInfo[] properties)
    {
        string[] requiredPropertyNames = properties.Where(prop => prop.GetCustomAttribute<RequiredAttribute>() is not null).Select(prop => prop.Name).ToArray();
        return requiredPropertyNames.Length == 0 
            ? null 
            : new RequiredKeyword(requiredPropertyNames);
    }

    private static BodyJsonSchema GenerateSchemaForEnum(Type type, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword { InstanceTypes = new[] { InstanceType.String } };

        IEnumerable<JsonInstanceElement> allowedStringEnums = type.GetEnumNames().Select(name => new JsonInstanceElement(JsonSerializer.SerializeToElement(name), ImmutableJsonPointer.Empty));
        var enumKeyword = new EnumKeyword(allowedStringEnums.ToList());

        var keywords = new List<KeywordBase> { typeKeyword, enumKeyword };
        keywords.AddRange(keywordsFromProperty);
        keywords.AddRange(GenerateKeywordsFromType(type));

        return new BodyJsonSchema(keywords);
    }

    /// <summary>
    /// Extract attributes from either header of <see cref="Type"/> itself or <see cref="PropertyInfo"/>
    /// </summary>
    private static KeywordBase[] GenerateKeywordsFromType(Type type)
    {
        IEnumerable<IKeywordGenerator> keywordGeneratorOnType = type.GetCustomAttributes().OfType<IKeywordGenerator>();
        return keywordGeneratorOnType.Select(keywordGenerator => keywordGenerator.CreateKeyword(type)).ToArray();
    }

    /// <summary>
    /// Extract attributes from either header of <see cref="Type"/> itself or <see cref="PropertyInfo"/>
    /// </summary>
    private static KeywordBase[] GenerateKeywordsFromPropertyInfo(PropertyInfo propertyInfo)
    {
        IEnumerable<IKeywordGenerator> keywordGeneratorOnType = propertyInfo.GetCustomAttributes().OfType<IKeywordGenerator>();
        return keywordGeneratorOnType.Select(keywordGenerator => keywordGenerator.CreateKeyword(propertyInfo.PropertyType)).ToArray();
    }

    private static BodyJsonSchema GenerateSchemaForStringDictionary(Type type, JsonSchemaGeneratorOptions options, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword { InstanceTypes = new[] { InstanceType.Object } };
        Type valueType = type.GetGenericArguments()[1];
        JsonSchema valueSchema = GenerateSchema(valueType, options, Array.Empty<KeywordBase>());

        JsonSchema propertySchema;
        if (valueSchema is JsonSchemaResource valueSchemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(valueType, valueSchemaResource);

            propertySchema = GenerateSchemaReference(valueType, Array.Empty<KeywordBase>());
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

    private static BodyJsonSchema GenerateSchemaForArray(Type type, JsonSchemaGeneratorOptions options, KeywordBase[] keywordsFromProperty)
    {
        List<KeywordBase> keywords = new List<KeywordBase> { new TypeKeyword { InstanceTypes = new[] { InstanceType.Array } } };
        keywords.AddRange(keywordsFromProperty);
        
        Type elementType = type.GetElementType()!;
        JsonSchema elementSchema = GenerateSchema(elementType, options, Array.Empty<KeywordBase>());

        JsonSchema itemsSchema;
        if (elementSchema is JsonSchemaResource elementSchemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(elementType, elementSchemaResource);

            itemsSchema = GenerateSchemaReference(elementType, Array.Empty<KeywordBase>());
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

    private static BodyJsonSchema GenerateSchemaReference(Type type, KeywordBase[] keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.ToList(), new List<ISchemaContainerValidationNode>(0), new SchemaReferenceKeyword(CreateRefUri(type)), null, null, null, null);
    }

    private static Uri CreateRefUri(Type type)
    {
        return new Uri("#" + new ImmutableJsonPointer(
            new[] { DefsKeyword.Keyword, TypeSchemaDefinitions.GetDefName(type) }), UriKind.Relative);

        // return new UriBuilder
        // {
        //     Fragment = new ImmutableJsonPointer(
        //         new[] { DefsKeyword.Keyword, TypeSchemaDefinitions.GetDefName(type) }).ToString()
        // }.Uri;
    }
}
