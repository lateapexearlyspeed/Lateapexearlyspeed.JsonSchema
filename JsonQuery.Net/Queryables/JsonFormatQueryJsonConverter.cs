using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public abstract class JsonFormatQueryJsonConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable
{
    protected static readonly string QueryKeyword = JsonQueryableRegistry.GetKeyword(typeof(TQuery));

    public override TQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Invalid json type for '{QueryKeyword}' query");
        }

        reader.Read();

        if (reader.TokenType != JsonTokenType.String || reader.GetString() != QueryKeyword)
        {
            throw new JsonException($"Invalid keyword name for '{QueryKeyword}' query");
        }

        reader.Read();

        TQuery query = ReadArguments(ref reader, typeToConvert, options);

        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw CreateUnexpectedArgumentException(reader);
        }

        return query;
    }

    [Pure]
    private static JsonException CreateUnexpectedArgumentException(Utf8JsonReader reader)
    {
        return new JsonException($"Unexpected array argument: '{JsonSerializer.Deserialize<JsonElement>(ref reader).GetRawText()}' in '{QueryKeyword}' query");
    }

    /// <summary>
    /// Should be implemented to read to end of JsonArray
    /// </summary>
    protected abstract TQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
}