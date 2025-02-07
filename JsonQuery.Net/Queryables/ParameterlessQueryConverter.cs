using System.Text.Json;

namespace JsonQuery.Net.Queryables;

public class ParameterlessQueryConverter<TQuery> : JsonFormatQueryJsonConverter<TQuery> where TQuery : IJsonQueryable, new()
{
    protected override TQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new TQuery();
    }

    public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.GetKeyword());
        writer.WriteEndArray();
    }
}

public class ParameterlessQueryParserConverter<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable, new()
{
    protected override TQuery ReadArguments(ref JsonQueryReader reader)
    {
        return new TQuery();
    }
}