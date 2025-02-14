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

        IJsonQueryConverter converter = (IJsonQueryConverter)Activator.CreateInstance(parserConverterType);

        return converter.Read(ref reader);
    }
}