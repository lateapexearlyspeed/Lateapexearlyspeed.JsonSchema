using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<PowQuery>))]
public class PowQuery : OperatorQuery
{
    internal const string Keyword = "pow";
    internal const string Operator = "^";

    public PowQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return Math.Pow((double)QueryLeftDecimal(data), (double)QueryRightDecimal(data));
    }
}