using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SubstringQueryConverter))]
[JsonQueryConverter(typeof(SubstringQueryParserConverter))]
public class SubstringQuery : IJsonQueryable
{
    internal const string Keyword = "substring";

    public IJsonQueryable SubQuery { get; }
    public int StartIdx { get; }
    public int? EndIdx { get; }

    public SubstringQuery(IJsonQueryable query, int startIdx, int? endIdx = null)
    {
        SubQuery = query;
        StartIdx = startIdx < 0 ? 0 : startIdx;
        EndIdx = endIdx;
    }

    public JsonNode? Query(JsonNode? data)
    {
        JsonNode? stringNode = SubQuery.Query(data);

        if (stringNode is null || stringNode.GetValueKind() != JsonValueKind.String)
        {
            return null;
        }

        string stringContent = stringNode.GetValue<string>();

        if (StartIdx >= stringContent.Length)
        {
            return string.Empty;
        }

        if (!EndIdx.HasValue || EndIdx.Value >= stringContent.Length)
        {
            return stringContent.Substring(StartIdx);
        }

        Debug.Assert(EndIdx.HasValue);

        if (StartIdx >= EndIdx.Value)
        {
            return string.Empty;
        }

        return stringContent.Substring(StartIdx, EndIdx.Value - StartIdx);
    }
}

internal class SubstringQueryParserConverter : JsonQueryConverter<SubstringQuery>
{
    public override SubstringQuery Read(ref JsonQueryReader reader)
    {
        reader.Read();
        reader.Read();

        IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        if (reader.TokenType != JsonQueryTokenType.Number)
        {
            throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(SubstringQuery)}", reader.Position);
        }

        int start = (int)reader.GetDecimal();

        reader.Read();

        int? end;

        if (reader.TokenType == JsonQueryTokenType.Number)
        {
            end = (int)reader.GetDecimal();
            reader.Read();
        }
        else
        {
            end = null;
        }

        return new SubstringQuery(query, start, end);
    }
}

internal class SubstringQueryConverter : JsonFormatQueryJsonConverter<SubstringQuery>
{
    protected override SubstringQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

        reader.Read();

        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Invalid json type {reader.TokenType} for 'substring' 's start value");
        }

        int start = (int)reader.GetDecimal();

        reader.Read();

        int? end;

        if (reader.TokenType == JsonTokenType.Number)
        {
            end = (int)reader.GetDecimal();
            reader.Read();
        }
        else
        {
            end = null;
        }

        return new SubstringQuery(query, start, end);
    }

    public override void Write(Utf8JsonWriter writer, SubstringQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubQuery);
        writer.WriteNumberValue(value.StartIdx);
        if (value.EndIdx.HasValue)
        {
            writer.WriteNumberValue(value.EndIdx.Value);
        }

        writer.WriteEndArray();
    }
}