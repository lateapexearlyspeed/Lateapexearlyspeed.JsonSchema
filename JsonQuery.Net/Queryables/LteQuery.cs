using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<LteQuery>))]
public class LteQuery : OperatorQuery
{
    internal const string Keyword = "lte";
    internal const string Operator = "<=";

    public LteQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftDecimal(data) <= QueryRightDecimal(data);
    }
}