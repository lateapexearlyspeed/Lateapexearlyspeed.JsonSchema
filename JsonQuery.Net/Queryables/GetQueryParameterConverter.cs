using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class GetQueryParameterConverter : JsonConverterFactory
{
    public static bool CanConvertInternal(Type typeToConvert)
    {
        return typeof(IJsonQueryable).IsAssignableFrom(typeToConvert)
               && typeToConvert.GetConstructor(new[] { typeof(GetQuery) }) is not null;
    }

    public override bool CanConvert(Type typeToConvert) => CanConvertInternal(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(GetQueryParameterConverterInner<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private class GetQueryParameterConverterInner<TQuery> : JsonFormatQueryJsonConverter<TQuery> where TQuery : IJsonQueryable, ISubGetQuery
    {
        protected override TQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            GetQuery getQuery = JsonSerializer.Deserialize<GetQuery>(ref reader)!;

            reader.Read();

            return (TQuery)Activator.CreateInstance(typeof(TQuery), getQuery);
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubGetQuery);

            writer.WriteEndArray();
        }
    }
}

public class GetQueryParameterParserConverter : IJsonQueryConverterFactory
{
    public bool CanConvert(Type typeToConvert) => GetQueryParameterConverter.CanConvertInternal(typeToConvert);

    public IJsonQueryConverter CreateConverter(Type typeToConvert)
    {
        Type converterType = typeof(GetQueryParameterParserConverterInner<>).MakeGenericType(typeToConvert);

        return (IJsonQueryConverter)Activator.CreateInstance(converterType);
    }

    private class GetQueryParameterParserConverterInner<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable
    {
        protected override TQuery ReadArguments(ref JsonQueryReader reader)
        {
            IJsonQueryable query = JsonQueryParser.ParseSingleQuery(ref reader);

            if (query is not GetQuery getQuery)
            {
                throw new JsonQueryParseException($"'{QueryKeyword}' function needs 'get' query as argument", reader.Position);
            }

            reader.Read();

            return (TQuery)Activator.CreateInstance(typeof(TQuery), getQuery);
        }
    }
}

