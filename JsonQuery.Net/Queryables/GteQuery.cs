using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<GteQuery>))]
public class GteQuery : OperatorQuery
{
    internal const string Keyword = "gte";
    internal const string Operator = ">=";

    public GteQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftDecimal(data) >= QueryRightDecimal(data);
    }
}