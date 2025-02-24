using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class SkipLastQuery : IJsonQueryable
{
    internal const string Keyword = "skipLast";

    [QueryArgument(0)]
    public int Count { get; }

    public SkipLastQuery(int count)
    {
        Count = count;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return new JsonArray(array.SkipLast(Count).Select(item => item?.DeepClone()).ToArray());
    }
}