using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<DivideQuery>))]
public class DivideQuery : OperatorQuery
{
    internal const string Keyword = "divide";
    internal const string Operator = "/";

    public DivideQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode? Query(JsonNode? data)
    {
        decimal leftDecimal = QueryLeftDecimal(data);
        decimal rightDecimal = QueryRightDecimal(data);

        return rightDecimal == 0 ? null : (leftDecimal / rightDecimal);
    }
}