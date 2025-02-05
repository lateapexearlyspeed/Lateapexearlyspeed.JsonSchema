using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(GetQueryParameterConverter<GroupByQuery>))]
[JsonQueryConverter(typeof(GetQueryParameterParserConverter<GroupByQuery>))]
public class GroupByQuery : IJsonQueryable, ISubGetQuery
{
    internal const string Keyword = "groupBy";

    public GroupByQuery(GetQuery getQuery)
    {
        SubGetQuery = getQuery;
    }

    public GetQuery SubGetQuery { get; }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        IEnumerable<IGrouping<string, JsonNode?>> groupByResult = array.GroupBy(itemNode =>
        {
            JsonNode? keyNode = SubGetQuery.Query(itemNode);

            if (keyNode is null || keyNode.GetValueKind() != JsonValueKind.String)
            {
                return string.Empty;
            }

            return keyNode.GetValue<string>();
        });

        IEnumerable<KeyValuePair<string, JsonNode?>> newProperties = groupByResult.Select(group => KeyValuePair.Create<string, JsonNode?>(group.Key, new JsonArray(group.Select(item => item?.DeepClone()).ToArray())));

        return new JsonObject(newProperties);
    }
}