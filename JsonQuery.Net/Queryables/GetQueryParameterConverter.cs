using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class GetQueryParameterConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        ConstructorInfo? constructorInfo = typeToConvert.GetConstructor(new[]{typeof(GetQuery)});

        return constructorInfo is not null;
    }

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

public class GetQueryParameterParserConverter<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable
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