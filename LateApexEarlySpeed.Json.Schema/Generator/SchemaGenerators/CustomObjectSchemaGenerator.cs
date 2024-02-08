using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class CustomObjectSchemaGenerator : ISchemaGenerator
{
    public BodyJsonSchema Generate(Type typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        TypeKeyword typeKeyword = typeToConvert.IsValueType
            ? new TypeKeyword(InstanceType.Object)
            : new TypeKeyword(InstanceType.Object, InstanceType.Null);

        IEnumerable<KeywordBase> keywordsOnType = SchemaGenerationHelper.GenerateKeywordsFromType(typeToConvert);

        PropertyInfo[] propertyInfos = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        FieldInfo[] fieldInfos = typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Instance);

        IEnumerable<MemberInfo> memberInfos = propertyInfos.Concat<MemberInfo>(fieldInfos);

        PropertiesKeyword propertiesKeyword = CreatePropertiesKeyword(memberInfos, options);

        RequiredKeyword? requiredKeyword = CreateRequiredKeyword(memberInfos, options);

        IEnumerable<KeywordBase> keywords = keywordsOnType.Append(typeKeyword).Append(propertiesKeyword);
        if (requiredKeyword is not null)
        {
            keywords = keywords.Append(requiredKeyword);
        }

        return new JsonSchemaResource(new Uri(typeToConvert.FullName!, UriKind.Relative), keywords.ToList(), new List<ISchemaContainerValidationNode>(0), null, null, null, null, null);
    }

    private static PropertiesKeyword CreatePropertiesKeyword(IEnumerable<MemberInfo> memberInfos, JsonSchemaGeneratorOptions options)
    {
        var propertiesSchemas = new Dictionary<string, JsonSchema>();

        foreach (MemberInfo memberInfo in memberInfos.Where(prop => prop.GetCustomAttribute<JsonIgnoreAttribute>() is null))
        {
            Type memberType = GetMemberType(memberInfo);

            KeywordBase[] keywordsOfMember = GenerateKeywordsFromMemberInfo(memberInfo);
            JsonSchema propertySchema = JsonSchemaGenerator.GenerateSchema(memberType, keywordsOfMember, options);

            if (propertySchema is JsonSchemaResource propertySchemaResource)
            {
                options.SchemaDefinitions.AddSchemaDefinition(memberType, propertySchemaResource);

                propertySchema = SchemaGenerationHelper.GenerateSchemaReference(memberType, keywordsOfMember, options.MainDocumentBaseUri!);
            }

            if (memberInfo.GetCustomAttribute<NotNullAttribute>() is not null)
            {
                var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array);
                var allOfKeyword = new AllOfKeyword(new List<JsonSchema> { propertySchema, new BodyJsonSchema(new List<KeywordBase> { typeKeyword }) });

                propertySchema = new BodyJsonSchema(new List<KeywordBase> { allOfKeyword });
            }

            propertiesSchemas[GetPropertyName(memberInfo, options)] = propertySchema;
        }

        return new PropertiesKeyword(propertiesSchemas);
    }

    private static string GetPropertyName(MemberInfo memberInfo, JsonSchemaGeneratorOptions options)
    {
        JsonPropertyNameAttribute? jsonPropertyNameAttribute = memberInfo.GetCustomAttribute<JsonPropertyNameAttribute>();

        return jsonPropertyNameAttribute is null
            ? options.PropertyNamingPolicy.ConvertName(memberInfo.Name)
            : jsonPropertyNameAttribute.Name;
    }

    private static RequiredKeyword? CreateRequiredKeyword(IEnumerable<MemberInfo> members, JsonSchemaGeneratorOptions options)
    {
        string[] requiredPropertyNames = members
            .Where(prop => prop.GetCustomAttribute<JsonRequiredAttribute>() is not null || prop.GetCustomAttribute<RequiredAttribute>() is not null)
            .Select(memberInfo => GetPropertyName(memberInfo, options)).ToArray();
        return requiredPropertyNames.Length == 0
            ? null
            : new RequiredKeyword(requiredPropertyNames);
    }

    /// <summary>
    /// Extract attributes from either <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>
    /// </summary>
    private static KeywordBase[] GenerateKeywordsFromMemberInfo(MemberInfo memberInfo)
    {
        IEnumerable<IKeywordGenerator> keywordGeneratorOnType = memberInfo.GetCustomAttributes().OfType<IKeywordGenerator>();
        return keywordGeneratorOnType.Select(keywordGenerator => keywordGenerator.CreateKeyword(GetMemberType(memberInfo))).ToArray();
    }

    private static Type GetMemberType(MemberInfo memberInfo)
    {
        Debug.Assert((memberInfo.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0);

        if ((memberInfo.MemberType & MemberTypes.Property) != 0)
        {
            Debug.Assert(memberInfo is PropertyInfo);
            return ((PropertyInfo)memberInfo).PropertyType;
        }

        Debug.Assert(memberInfo is FieldInfo);
        return ((FieldInfo)memberInfo).FieldType;
    }
}