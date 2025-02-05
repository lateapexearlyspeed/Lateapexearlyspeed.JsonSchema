using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<AndQuery>))]
public class AndQuery : OperatorQuery
{
    internal const string Keyword = "and";
    internal const string Operator = "and";

    public AndQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftBoolean(data) && QueryRightBoolean(data);
    }
}