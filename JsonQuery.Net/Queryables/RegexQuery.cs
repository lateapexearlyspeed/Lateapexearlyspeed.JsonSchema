using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(RegexQueryConverter))]
[JsonQueryConverter(typeof(RegexQueryParserConverter))]
public class RegexQuery : IJsonQueryable
{
    internal const string Keyword = "regex";

    public IJsonQueryable SubQuery { get; }
    public string RegexValue { get; }
    public RegexOptions Options { get; }

    public RegexQuery(IJsonQueryable query, string regex, RegexOptions options)
    {
        SubQuery = query;
        RegexValue = regex;
        Options = options;
    }

    public JsonNode? Query(JsonNode? data)
    {
        JsonNode? jsonNode = SubQuery.Query(data);

        if (jsonNode is null || jsonNode.GetValueKind() != JsonValueKind.String)
        {
            return null;
        }

        string content = jsonNode.GetValue<string>();

        return JsonValue.Create(Regex.IsMatch(content, RegexValue, Options));
    }
}

public class RegexQueryParserConverter : JsonQueryConverter<RegexQuery>
{
    public override RegexQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        if (reader.TokenType != JsonQueryTokenType.String)
        {
            throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(RegexQuery)}", reader.Position);
        }

        string regex = reader.GetString();

        reader.Read();

        RegexOptions option = RegexOptions.None;
        if (reader.TokenType == JsonQueryTokenType.String)
        {
            if (reader.GetString() == "i")
            {
                option = RegexOptions.IgnoreCase;
            }

            reader.Read();
        }

        return new RegexQuery(query, regex, option);
    }
}

public class RegexQueryConverter : JsonConverter<RegexQuery>
{
    public override RegexQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();

        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

        reader.Read();

        string regex = reader.GetString()!;

        reader.Read();

        RegexOptions option = RegexOptions.None;
        if (reader.TokenType == JsonTokenType.String)
        {
            if (reader.GetString() == "i")
            {
                option = RegexOptions.IgnoreCase;
            }

            reader.Read();
        }

        return new RegexQuery(query, regex, option);
    }

    public override void Write(Utf8JsonWriter writer, RegexQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubQuery);
        writer.WriteStringValue(value.RegexValue);

        if (value.Options == RegexOptions.IgnoreCase)
        {
            writer.WriteStringValue("i");
        }

        writer.WriteEndArray();
    }
}