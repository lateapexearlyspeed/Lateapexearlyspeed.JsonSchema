using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
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
        BodyJsonSchema jsonSchema = GenerateSchema(type, options, Array.Empty<KeywordBase>());

        BodyJsonSchemaDocument bodyJsonSchemaDocument = jsonSchema.TransformToSchemaDocument(new Uri(BodyJsonSchemaDocument.DefaultDocumentBaseUri, type.FullName!), new DefsKeyword(options.SchemaDefinitions.GetAll().ToDictionary(kv => kv.Key, kv => kv.Value as JsonSchema)));

        return new JsonValidator(bodyJsonSchemaDocument);
    }

    private static BodyJsonSchema GenerateSchema(Type type, JsonSchemaGeneratorOptions options, KeywordBase[] keywordsFromProperty)
    {
        JsonSchemaResource? schemaDefinition = options.SchemaDefinitions.GetSchemaDefinition(type);

        if (schemaDefinition is not null)
        {
            return schemaDefinition;
        }

        // Signed Integer
        if (type == typeof(int))
        {
            return GenerateSchemaForSignedInteger(keywordsFromProperty, int.MinValue, int.MaxValue);
        }

        if (type == typeof(long))
        {
            return GenerateSchemaForSignedInteger(keywordsFromProperty, long.MinValue, long.MaxValue);
        }

        if (type == typeof(short))
        {
            return GenerateSchemaForSignedInteger(keywordsFromProperty, short.MinValue, short.MaxValue);
        }

        if (type == typeof(sbyte))
        {
            return GenerateSchemaForSignedInteger(keywordsFromProperty, sbyte.MinValue, sbyte.MaxValue);
        }

        // Unsigned integer
        if (type == typeof(uint))
        {
            return GenerateSchemaForUnsignedInteger(keywordsFromProperty, uint.MaxValue);
        }

        if (type == typeof(ulong))
        {
            return GenerateSchemaForUnsignedInteger(keywordsFromProperty, ulong.MaxValue);
        }

        if (type == typeof(ushort))
        {
            return GenerateSchemaForUnsignedInteger(keywordsFromProperty, ushort.MaxValue);
        }

        if (type == typeof(byte))
        {
            return GenerateSchemaForUnsignedInteger(keywordsFromProperty, byte.MaxValue);
        }

        // Floating-point numeric types
        if (type == typeof(float))
        {
            return GenerateSchemaForDouble(keywordsFromProperty, float.MinValue, float.MaxValue);
        }

        if (type == typeof(double))
        {
            return GenerateSchemaForDouble(keywordsFromProperty, double.MinValue, double.MaxValue);
        }

        if (type == typeof(decimal))
        {
            return GenerateSchemaForDecimal(keywordsFromProperty);
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

        // Dictionary<string, TValue>
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) && type.GetGenericArguments()[0] == typeof(string))
        {
            return GenerateSchemaForStringDictionary(type, options, keywordsFromProperty);
        }

        // Collection
        if (type.GetInterface("IEnumerable`1") is not null)
        {
            return GenerateSchemaForCollection(type, options, keywordsFromProperty);
        }

        // Enum
        if (type.IsEnum)
        {
            return GenerateSchemaForEnum(type, keywordsFromProperty);
        }

        if (type == typeof(Guid))
        {
            return GenerateSchemaForGuid(keywordsFromProperty);
        }

        if (type == typeof(Uri))
        {
            return GenerateSchemaForUri(keywordsFromProperty);
        }

        if (type == typeof(DateTimeOffset))
        {
            return GenerateSchemaForDateTimeOffset(keywordsFromProperty);
        }

        // Nullable value type
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return GenerateSchemaForNullableValueType(type, options, keywordsFromProperty);
        }

        // Custom object
        return GenerateSchemaForCustomObject(type, options);
    }

    private static BodyJsonSchema GenerateSchemaForUnsignedInteger(KeywordBase[] keywordsFromProperty, ulong max)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Integer)).Append(new MinimumKeyword(0)).Append(new MaximumKeyword(max)).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForDateTimeOffset(KeywordBase[] keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword(InstanceType.String);
        var formatKeyword = new FormatKeyword(DateTimeFormatValidator.FormatName);

        return new BodyJsonSchema(new List<KeywordBase>(keywordsFromProperty) { typeKeyword, formatKeyword });
    }

    private static BodyJsonSchema GenerateSchemaForUri(KeywordBase[] keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword(InstanceType.String, InstanceType.Null);
        var formatKeyword = new FormatKeyword(UriReferenceFormatValidator.FormatName);

        return new BodyJsonSchema(new List<KeywordBase>(keywordsFromProperty) { typeKeyword, formatKeyword });
    }

    private static BodyJsonSchema GenerateSchemaForGuid(KeywordBase[] keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword(InstanceType.String);
        var formatKeyword = new FormatKeyword(GuidFormatValidator.FormatName);

        return new BodyJsonSchema(new List<KeywordBase>(keywordsFromProperty) { typeKeyword, formatKeyword });
    }

    private static BodyJsonSchema GenerateSchemaForNullableValueType(Type type, JsonSchemaGeneratorOptions options, KeywordBase[] keywordsFromProperty)
    {
        Debug.Assert(type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));

        Type underlyingType = Nullable.GetUnderlyingType(type)!;

        BodyJsonSchema underlyingSchema = GenerateSchema(underlyingType, options, keywordsFromProperty);

        BodyJsonSchema nullTypeSchema = new BodyJsonSchema(new List<KeywordBase>
        {
            new TypeKeyword(InstanceType.Null)
        });

        if (underlyingSchema is JsonSchemaResource schemaResource)
        {
            options.SchemaDefinitions.AddSchemaDefinition(underlyingType, schemaResource);
            underlyingSchema = GenerateSchemaReference(underlyingType, keywordsFromProperty);
        }

        var anyOfKeyword = new AnyOfKeyword(new List<JsonSchema> { nullTypeSchema, underlyingSchema});

        return new BodyJsonSchema(new List<KeywordBase> { anyOfKeyword });
    }

    private static JsonSchemaResource GenerateSchemaForCustomObject(Type type, JsonSchemaGeneratorOptions options)
    {
        var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.Null);

        IEnumerable<KeywordBase> keywordsOnType = GenerateKeywordsFromType(type);

        PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

        PropertiesKeyword propertiesKeyword = CreatePropertiesKeyword(propertyInfos, options);

        RequiredKeyword? requiredKeyword = CreateRequiredKeyword(propertyInfos, options);

        IEnumerable<KeywordBase> keywords = keywordsOnType.Append(typeKeyword).Append(propertiesKeyword);
        if (requiredKeyword is not null)
        {
            keywords = keywords.Append(requiredKeyword);
        }

        return new JsonSchemaResource(new Uri(type.FullName!, UriKind.Relative), keywords.ToList(), new List<ISchemaContainerValidationNode>(0), null, null, null, null, null);
    }

    private static PropertiesKeyword CreatePropertiesKeyword(PropertyInfo[] propertyInfos, JsonSchemaGeneratorOptions options)
    {
        var propertiesSchemas = new Dictionary<string, JsonSchema>();

        foreach (PropertyInfo propertyInfo in propertyInfos.Where(prop => prop.GetCustomAttribute<JsonSchemaIgnoreAttribute>() is null))
        {
            KeywordBase[] keywordsOfProp = GenerateKeywordsFromPropertyInfo(propertyInfo);
            JsonSchema propertySchema = GenerateSchema(propertyInfo.PropertyType, options, keywordsOfProp);

            if (propertySchema is JsonSchemaResource propertySchemaResource)
            {
                options.SchemaDefinitions.AddSchemaDefinition(propertyInfo.PropertyType, propertySchemaResource);

                propertySchema = GenerateSchemaReference(propertyInfo.PropertyType, keywordsOfProp);
            }

            if (propertyInfo.GetCustomAttribute<NotNullAttribute>() is not null)
            {
                var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array);
                var allOfKeyword = new AllOfKeyword(new List<JsonSchema>{ propertySchema, new BodyJsonSchema(new List<KeywordBase>{typeKeyword})});
                
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
        string[] requiredPropertyNames = properties.Where(prop => prop.GetCustomAttribute<RequiredAttribute>() is not null).Select(propertyInfo => GetPropertyName(propertyInfo, options)).ToArray();
        return requiredPropertyNames.Length == 0 
            ? null 
            : new RequiredKeyword(requiredPropertyNames);
    }

    private static BodyJsonSchema GenerateSchemaForEnum(Type type, IEnumerable<KeywordBase> keywordsFromProperty)
    {
        var typeKeyword = new TypeKeyword(InstanceType.String);

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
        var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.Null);
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

    private static BodyJsonSchema GenerateSchemaForCollection(Type type, JsonSchemaGeneratorOptions options, KeywordBase[] keywordsFromProperty)
    {
        List<KeywordBase> keywords = new List<KeywordBase> { new TypeKeyword(InstanceType.Array, InstanceType.Null) };
        keywords.AddRange(keywordsFromProperty);

        Debug.Assert(type.GetInterface("IEnumerable`1") is not null);
        Type elementType = type.GetInterface("IEnumerable`1").GetGenericArguments()[0];
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
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.String, InstanceType.Null)).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForBoolean(IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Boolean)).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForDouble(IEnumerable<KeywordBase> keywordsFromProperty, double min, double max)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Number)).Append(new MinimumKeyword(min)).Append(new MaximumKeyword(max)).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForDecimal(IEnumerable<KeywordBase> keywordsFromProperty)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Number)).Append(new MinimumKeyword(decimal.MinValue)).Append(new MaximumKeyword(decimal.MaxValue)).ToList());
    }

    private static BodyJsonSchema GenerateSchemaForSignedInteger(IEnumerable<KeywordBase> keywordsFromProperty, long min, long max)
    {
        return new BodyJsonSchema(keywordsFromProperty.Append(new TypeKeyword(InstanceType.Integer)).Append(new MinimumKeyword(min)).Append(new MaximumKeyword(max)).ToList());
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
