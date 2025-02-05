using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<GtQuery>))]
public class GtQuery : OperatorQuery
{
    internal const string Keyword = "gt";
    internal const string Operator = ">";

    public GtQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftDecimal(data) > QueryRightDecimal(data);
    }
}