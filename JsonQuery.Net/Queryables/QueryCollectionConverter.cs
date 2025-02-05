using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class QueryCollectionConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, IMultipleSubQuery
{
    public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read(); // pass start array token
        reader.Read(); // pass keyword

        var queries = new List<IJsonQueryable>();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            queries.Add(JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!);

            reader.Read();
        }

        return (TQuery)Activator.CreateInstance(typeof(TQuery), new object[] { queries.ToArray() });
    }

    public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        foreach (IJsonQueryable subQuery in value.SubQueries)
        {
            JsonSerializer.Serialize(writer, subQuery);
        }

        writer.WriteEndArray();
    }
}

public class QueryCollectionParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable
{
    public override TQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        var queries = new List<IJsonQueryable>();
        while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
        {
            queries.Add(JsonQueryParser.ParseQueryCombination(ref reader));
            reader.Read();
        }

        return (TQuery)Activator.CreateInstance(typeof(TQuery), new object[] { queries.ToArray() });
    }
}