using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class GetQueryParameterConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, ISubGetQuery
{
    public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        GetQuery getQuery = JsonSerializer.Deserialize<GetQuery>(ref reader)!;

        reader.Read();

        return (TQuery?)Activator.CreateInstance(typeof(TQuery), getQuery);
    }

    public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubGetQuery);

        writer.WriteEndArray();
    }
}

public class GetQueryParameterParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable
{
    public override TQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        GetQuery getQuery = (GetQuery)JsonQueryParser.ParseSingleQuery(ref reader);

        reader.Read();

        return (TQuery)Activator.CreateInstance(typeof(TQuery), getQuery);
    }
}