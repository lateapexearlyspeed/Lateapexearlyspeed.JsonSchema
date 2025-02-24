using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(RoundQueryConverter))]
[JsonQueryConverter(typeof(RoundQueryParserConverter))]
public class RoundQuery : IJsonQueryable
{
    internal const string Keyword = "round";

    public IJsonQueryable SubQuery { get; }
    public int Digits { get; }

    public RoundQuery(IJsonQueryable query, int digits = 0)
    {
        SubQuery = query;
        Digits = digits;
    }

    public JsonNode? Query(JsonNode? data)
    {
        JsonNode? numericNode = SubQuery.Query(data);

        if (numericNode is null || numericNode.GetValueKind() != JsonValueKind.Number)
        {
            return null;
        }

        decimal value = numericNode.GetValue<decimal>();
        decimal roundResult = Math.Round(value, Digits, MidpointRounding.AwayFromZero);

        return JsonValue.Create(roundResult);
    }
}

public class RoundQueryParserConverter : JsonQueryFunctionConverter<RoundQuery>
{
    protected override RoundQuery ReadArguments(ref JsonQueryReader reader)
    {
        IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        int digits = 0;
        if (reader.TokenType == JsonQueryTokenType.Number)
        {
            digits = (int)reader.GetDecimal();
            reader.Read();
        }

        return new RoundQuery(query, digits);
    }
}

public class RoundQueryConverter : JsonFormatQueryJsonConverter<RoundQuery>
{
    protected override RoundQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader, options)!;

        reader.Read();

        int digits = 0;
        if (reader.TokenType == JsonTokenType.Number)
        {
            digits = (int)reader.GetDecimal();
            reader.Read();
        }

        return new RoundQuery(query, digits);
    }

    public override void Write(Utf8JsonWriter writer, RoundQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubQuery, options);
        if (value.Digits != 0)
        {
            writer.WriteNumberValue(value.Digits);
        }

        writer.WriteEndArray();
    }
}