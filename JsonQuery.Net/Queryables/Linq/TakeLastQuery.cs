using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class TakeLastQuery : IJsonQueryable
{
    internal const string Keyword = "takeLast";

    [QueryArgument(0)]
    public int Count { get; }

    public TakeLastQuery(int count)
    {
        Count = count;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return new JsonArray(array.TakeLast(Count).Select(item => item?.DeepClone()).ToArray());
    }
}