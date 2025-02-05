using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<ModQuery>))]
public class ModQuery : OperatorQuery
{
    internal const string Keyword = "mod";
    internal const string Operator = "%";

    public ModQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode Query(JsonNode? data)
    {
        return QueryLeftDecimal(data) % QueryRightDecimal(data);
    }
}