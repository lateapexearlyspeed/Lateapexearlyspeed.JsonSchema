using System.Reflection;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

internal static class FunctionQuerySerializer
{
    public static IJsonQueryable Deserialize(ref JsonQueryReader reader, Type queryType)
    {
        JsonQueryConverterAttribute? converterAttribute = queryType.GetCustomAttribute<JsonQueryConverterAttribute>();

        if (converterAttribute is not null)
        {
            return DeserializeFromConverterAttribute(ref reader, converterAttribute, queryType);
        }

        return DeserializeByDefaultContract(ref reader, queryType);
    }

    private static IJsonQueryable DeserializeByDefaultContract(ref JsonQueryReader reader, Type queryType)
    {
        ConstructorInfo[] constructors = queryType.GetConstructors();

        if (constructors.Length != 1)
        {
            throw new JsonQueryParseException($"Cannot deserialize type '{queryType}' because of multiple constructors", reader.Position);
        }

        ParameterInfo[] parameterInfos = constructors[0].GetParameters();

        var arguments = new object[parameterInfos.Length];

        reader.Read(); // skip FunctionName
        reader.Read(); // skip StartParenthesis

        for (int i = 0; i < arguments.Length; i++)
        {
            arguments[i] = JsonQueryParser.Deserialize(ref reader, parameterInfos[i].ParameterType);

            reader.Read();
        }

        if (reader.TokenType != JsonQueryTokenType.EndParenthesis)
        {
            throw new JsonQueryParseException($"Error during json query parsing for '{queryType}'", reader.Position);
        }

        return (IJsonQueryable)Activator.CreateInstance(queryType, arguments);
    }

    private static IJsonQueryable DeserializeFromConverterAttribute(ref JsonQueryReader reader, JsonQueryConverterAttribute converterAttribute, Type queryType)
    {
        Type parserConverterType = converterAttribute.ParserType;

        if (!typeof(IJsonQueryConverter).IsAssignableFrom(parserConverterType) && !typeof(IJsonQueryConverterFactory).IsAssignableFrom(parserConverterType))
        {
            throw new NotSupportedException($"Parser type:{parserConverterType} is invalid.");
        }

        IJsonQueryTypeChecker converterOrFactory = (IJsonQueryTypeChecker)Activator.CreateInstance(parserConverterType);

        if (!converterOrFactory.CanConvert(queryType))
        {
            throw new NotSupportedException($"Query type: {queryType} is decorated with {parserConverterType} but it cannot convert {queryType}");
        }

        IJsonQueryConverter converter;

        if (converterOrFactory is IJsonQueryConverterFactory factory)
        {
            converter = factory.CreateConverter(queryType);
        }
        else
        {
            converter = (IJsonQueryConverter)converterOrFactory;
        }

        return converter.Read(ref reader);
    }
}