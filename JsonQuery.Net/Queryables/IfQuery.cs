using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(IfQueryConverter))]
[JsonQueryConverter(typeof(IfQueryParserConverter))]
public class IfQuery : IJsonQueryable
{
    internal const string Keyword = "if";

    public IJsonQueryable IfSubQuery { get; }
    public IJsonQueryable ThenSubQuery { get; }
    public IJsonQueryable ElseSubQuery { get; }

    public IfQuery(IJsonQueryable ifQuery, IJsonQueryable thenQuery, IJsonQueryable elseQuery)
    {
        IfSubQuery = ifQuery;
        ThenSubQuery = thenQuery;
        ElseSubQuery = elseQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        bool condition = IfSubQuery.Query(data).GetBooleanValue();

        return condition ? ThenSubQuery.Query(data) : ElseSubQuery.Query(data);
    }
}

public class IfQueryParserConverter : JsonQueryConverter<IfQuery>
{
    public override IfQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        IJsonQueryable ifQuery = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        IJsonQueryable thenQuery = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        IJsonQueryable elseQuery = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        return new IfQuery(ifQuery, thenQuery, elseQuery);
    }
}

public class IfQueryConverter : JsonFormatQueryJsonConverter<IfQuery>
{
    protected override IfQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable ifQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

        reader.Read();

        IJsonQueryable thenQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

        reader.Read();

        IJsonQueryable elseQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

        reader.Read();

        return new IfQuery(ifQuery, thenQuery, elseQuery);
    }

    public override void Write(Utf8JsonWriter writer, IfQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.IfSubQuery);
        JsonSerializer.Serialize(writer, value.ThenSubQuery);
        JsonSerializer.Serialize(writer, value.ElseSubQuery);

        writer.WriteEndArray();
    }
}