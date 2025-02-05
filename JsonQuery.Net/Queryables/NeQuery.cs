using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<NeQuery>))]
public class NeQuery : OperatorQuery
{
    internal const string Keyword = "ne";
    internal const string Operator = "!=";

    public NeQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return !JsonNode.DeepEquals(Left.Query(data), Right.Query(data));
    }
}