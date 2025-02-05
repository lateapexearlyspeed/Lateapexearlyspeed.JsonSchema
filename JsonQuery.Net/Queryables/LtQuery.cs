using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<LtQuery>))]
public class LtQuery : OperatorQuery
{
    internal const string Keyword = "lt";
    internal const string Operator = "<";

    public LtQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftDecimal(data) < QueryRightDecimal(data);
    }
}