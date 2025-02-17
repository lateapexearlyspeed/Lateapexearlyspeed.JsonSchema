using System.Reflection;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

internal static class FunctionQuerySerializer
{
    public static IJsonQueryable Deserialize(ref JsonQueryReader reader, Type queryType)
    {
        JsonQueryConverterAttribute? converterAttribute = queryType.GetCustomAttribute<JsonQueryConverterAttribute>();

        if (converterAttribute is null)
        {
            throw new NotSupportedException($"Query type: {queryType} needs to be decorated with {nameof(JsonQueryConverterAttribute)}");
        }

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