using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator.SchemaGenerators;

internal class EnumSchemaGenerationCandidate : ISchemaGenerationCandidate
{
    public bool CanGenerate(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public BodyJsonSchema Generate(IType typeToConvert, IEnumerable<KeywordBase> keywordsFromProperty, JsonSchemaGeneratorOptions options)
    {
        IEnumerable<JsonInstanceElement> allowedStringEnums = typeToConvert.Type.GetEnumNames().Select(name => JsonInstanceSerializer.SerializeToElement(name));

        IEnumerable<JsonInstanceElement> enumCollection;
        if (HasJsonStringEnumConverter(typeToConvert.Type))
        {
            enumCollection = allowedStringEnums;
        }
        else
        {
            IEnumerable<JsonInstanceElement> allowedNumberEnums = typeToConvert.Type.GetEnumValues().Select(JsonInstanceSerializer.SerializeToElement);
            enumCollection = allowedStringEnums.Concat(allowedNumberEnums);
        }
        
        var enumKeyword = new EnumKeyword(enumCollection);

        var keywords = new List<KeywordBase> { enumKeyword };
        keywords.AddRange(keywordsFromProperty);
        keywords.AddRange(SchemaGenerationHelper.GenerateKeywordsFromType(typeToConvert.Type));

        return new BodyJsonSchema(keywords);
    }

    public static bool HasJsonStringEnumConverter(MemberInfo memberInfo)
    {
        return memberInfo.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType == typeof(JsonStringEnumConverter);
    }
}