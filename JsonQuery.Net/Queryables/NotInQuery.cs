using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<NotInQuery>))]
public class NotInQuery : OperatorQuery
{
    internal const string Keyword = "not in";
    internal const string Operator = "not in";

    public NotInQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
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

        return !array.Any(item => JsonNode.DeepEquals(item, value));
    }
}