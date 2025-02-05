using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<SubtractQuery>))]
public class SubtractQuery : OperatorQuery
{
    internal const string Keyword = "subtract";
    internal const string Operator = "-";

    public SubtractQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftDecimal(data) - QueryRightDecimal(data);
    }
}