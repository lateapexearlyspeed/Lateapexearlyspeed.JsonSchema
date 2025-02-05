using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<InQuery>))]
public class InQuery : OperatorQuery
{
    internal const string Keyword = "in";
    internal const string Operator = "in";

    public InQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode? Query(JsonNode? data)
    {
        JsonNode? rightArray = Right.Query(data);
        if (rightArray is not JsonArray array)
        {
            return null;
        }

        JsonNode? value = Left.Query(data);

        return array.Any(item => JsonNode.DeepEquals(item, value));
    }
}