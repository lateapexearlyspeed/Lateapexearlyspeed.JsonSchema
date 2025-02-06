using System.Text.Json;

namespace JsonQuery.Net.Queryables;

public class SingleQueryParameterConverter<TQuery> : JsonFormatQueryJsonConverter<TQuery> where TQuery : IJsonQueryable, ISingleSubQuery
{
    protected override TQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

        reader.Read();

        return (TQuery)Activator.CreateInstance(typeof(TQuery), query);
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