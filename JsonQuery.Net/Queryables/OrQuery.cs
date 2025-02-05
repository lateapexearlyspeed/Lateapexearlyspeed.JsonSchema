using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<OrQuery>))]
public class OrQuery : OperatorQuery
{
    internal const string Keyword = "or";
    internal const string Operator = "or";

    public OrQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftBoolean(data) || QueryRightBoolean(data);
    }
}