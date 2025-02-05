using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables;

public class ConstQueryable : IJsonQueryable
{
    public JsonNode? Value { get; }

    public ConstQueryable(JsonNode? value)
    {
        Value = value;
    }

    public JsonNode? Query(JsonNode? data)
    {
        return Value;
    }
}