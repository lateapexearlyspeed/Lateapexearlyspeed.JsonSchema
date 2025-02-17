using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter))]
public class AbsQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "abs";

    public AbsQuery(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public IJsonQueryable SubQuery { get; }

    public JsonNode? Query(JsonNode? data)
    {
        JsonNode? numericNode = SubQuery.Query(data);
        if (numericNode is null || numericNode.GetValueKind() != JsonValueKind.Number)
        {
            return null;
        }

        decimal value = numericNode.GetValue<decimal>();

        return JsonValue.Create(Math.Abs(value));
    }
}