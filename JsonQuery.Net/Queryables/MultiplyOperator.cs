using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<MultiplyOperator>))]
public class MultiplyOperator : OperatorQuery
{
    internal const string Keyword = "multiply";
    internal const string Operator = "*";

    public MultiplyOperator(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftDecimal(data) * QueryRightDecimal(data);
    }
}