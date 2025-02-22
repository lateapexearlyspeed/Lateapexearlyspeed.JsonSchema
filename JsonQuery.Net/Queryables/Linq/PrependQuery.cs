using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class PrependQuery : IJsonQueryable
{
    internal const string Keyword = "prepend";

    public IJsonQueryable ArrayQuery { get; }
    public IJsonQueryable PrependedElementQuery { get; }

    public PrependQuery(IJsonQueryable arrayQuery, IJsonQueryable prependedElementQuery)
    {
        ArrayQuery = arrayQuery;
        PrependedElementQuery = prependedElementQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (ArrayQuery.Query(data) is not JsonArray array)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = array.Prepend(PrependedElementQuery.Query(data));

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}