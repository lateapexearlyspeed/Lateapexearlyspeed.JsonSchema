using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class GroupByQuery : IJsonQueryable
{
    internal const string Keyword = "groupByLinq";

    [QueryParam(0)]
    public IJsonQueryable KeySelector { get; }

    [QueryParam(1)]
    public IJsonQueryable ElementSelector { get; }

    public GroupByQuery(IJsonQueryable keySelector, IJsonQueryable elementSelector)
    {
        KeySelector = keySelector;
        ElementSelector = elementSelector;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        IEnumerable<IGrouping<JsonNode?, JsonNode?>> groupByResult = array.GroupBy(item => KeySelector.Query(item), item => ElementSelector.Query(item), new JsonNodeEqualityComparer());

        JsonNode[] jsonObjects = groupByResult.Select<IGrouping<JsonNode?, JsonNode?>, JsonNode>(
            group =>
        {
            var properties = new[] { KeyValuePair.Create("key", group.Key?.DeepClone()), KeyValuePair.Create<string, JsonNode?>("value", new JsonArray(group.Select(groupItem => groupItem?.DeepClone()).ToArray())) };
            return new JsonObject(properties);
        }).ToArray();

        return new JsonArray(jsonObjects);
    }
}