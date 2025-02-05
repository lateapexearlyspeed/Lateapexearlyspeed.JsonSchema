using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class SingleQueryParameterConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, ISingleSubQuery
{
    public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

        TQuery? queryable = (TQuery?)Activator.CreateInstance(typeof(TQuery), query);

        reader.Read();

        return queryable;
    }

    public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubQuery);
        writer.WriteEndArray();
    }
}

public class SingleQueryParameterParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable
{
    public override TQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read(); // ')'

        return (TQuery)Activator.CreateInstance(typeof(TQuery), query);
    }
}