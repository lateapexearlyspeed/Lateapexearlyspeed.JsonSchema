using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class CustomObjectSchemaGenerator : ISchemaGenerator
{
    public BodyJsonSchema Generate(IType typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        TypeKeyword typeKeyword = typeToConvert.Type.IsValueType
            ? new TypeKeyword(InstanceType.Object)
            : new TypeKeyword(InstanceType.Object, InstanceType.Null);

        IEnumerable<KeywordBase> keywordsOnType = SchemaGenerationHelper.GenerateKeywordsFromType(typeToConvert.Type);

        IPropertyInfo[] propertyInfos = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        IFieldInfo[] fieldInfos = typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Instance);

        IEnumerable<IMemberInfo> memberInfos = propertyInfos.Concat<IMemberInfo>(fieldInfos);

        PropertiesKeyword propertiesKeyword = CreatePropertiesKeyword(memberInfos, options);

        RequiredKeyword? requiredKeyword = CreateRequiredKeyword(memberInfos.Select(m => m.MemberInfo), options);

        IEnumerable<KeywordBase> keywords = keywordsOnType.Append(typeKeyword).Append(propertiesKeyword);
        if (requiredKeyword is not null)
        {
            keywords = keywords.Append(requiredKeyword);
        }

        if (options.NullabilityTypeInfo.ReferenceTypeNullabilityPolicy.UseNullabilityAnnotation && typeToConvert.Type.IsGenericType) // Because same runtime generic types may have different nullabilities of their generic arguments, so if enable nullability annotation, we cannot make sure they can reuse same JsonSchemaResource (thus $ref)
        {
            return new BodyJsonSchema(keywords);
        }

        return new JsonSchemaResource(new Uri(typeToConvert.Type.FullName!, UriKind.Relative), keywords, new List<ISchemaContainerValidationNode>(0), null, null, null, null, null, null);
    }

    private static PropertiesKeyword CreatePropertiesKeyword(IEnumerable<IMemberInfo> memberInfos, JsonSchemaGeneratorOptions options)
    {
        var propertiesSchemas = new Dictionary<string, JsonSchema>();

        foreach (IMemberInfo memberInfo in memberInfos.Where(prop => prop.MemberInfo.GetCustomAttribute<JsonIgnoreAttribute>() is null))
        {
            IType memberType = memberInfo.GetMemberType();

            KeywordBase[] keywordsOfMember = GenerateKeywordsFromMemberInfo(memberInfo);
            JsonSchema propertySchema = JsonSchemaGenerator.GenerateSchema(memberType, keywordsOfMember, options);

            if (propertySchema is JsonSchemaResource propertySchemaResource)
            {
                options.SchemaDefinitions.AddSchemaDefinition(memberType.Type, propertySchemaResource);

                propertySchema = SchemaGenerationHelper.GenerateSchemaReference(memberType.Type, keywordsOfMember, options.MainDocumentBaseUri!);
            }

            List<KeywordBase>? keywordsForAdditionalAttributes = null;

            if (!memberType.Type.IsValueType && options.NullabilityTypeInfo.ReferenceTypeNullabilityPolicy.GetNullabilityState(memberInfo) == NullabilityState.NotNull)
            {
                var typeKeyword = new TypeKeyword(InstanceType.Object, InstanceType.String, InstanceType.Number, InstanceType.Boolean, InstanceType.Array);
                keywordsForAdditionalAttributes = new List<KeywordBase>(2) { typeKeyword };
            }

            if (memberType.Type.IsEnum && EnumSchemaGenerationCandidate.HasJsonStringEnumConverter(memberInfo.MemberInfo))
            {
                keywordsForAdditionalAttributes ??= new List<KeywordBase>(1);
                keywordsForAdditionalAttributes.Add(new EnumKeyword(memberType.Type.GetEnumNames().Select(JsonInstanceSerializer.SerializeToElement)));
            }

            if (keywordsForAdditionalAttributes is not null)
            {
                var allOfKeyword = new AllOfKeyword(new[] { propertySchema, new BodyJsonSchema(keywordsForAdditionalAttributes) });

                propertySchema = new BodyJsonSchema(new KeywordBase[] { allOfKeyword });
            }

            propertiesSchemas[GetPropertyName(memberInfo.MemberInfo, options)] = propertySchema;
        }

        return new PropertiesKeyword(propertiesSchemas, false);
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
            .Where(prop => prop.GetCustomAttribute<RequiredAttribute>() is not null)
            .Select(memberInfo => GetPropertyName(memberInfo, options)).ToArray();
        return requiredPropertyNames.Length == 0
            ? null
            : new RequiredKeyword(requiredPropertyNames, false);
    }

    /// <summary>
    /// Extract attributes from either <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>
    /// </summary>
    private static KeywordBase[] GenerateKeywordsFromMemberInfo(IMemberInfo memberInfo)
    {
        IEnumerable<IKeywordGenerator> keywordGeneratorOnType = memberInfo.MemberInfo.GetCustomAttributes().OfType<IKeywordGenerator>();
        return keywordGeneratorOnType.Select(keywordGenerator => keywordGenerator.CreateKeyword(memberInfo.GetMemberType().Type)).ToArray();
    }
}