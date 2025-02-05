using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<EqQuery>))]
public class EqQuery : OperatorQuery
{
    internal const string Keyword = "eq";
    internal const string Operator = "==";

    public EqQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        JsonNode? left = Left.Query(data);
        JsonNode? right = Right.Query(data);

        return JsonNode.DeepEquals(left, right);
    }
}