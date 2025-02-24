using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class SkipQuery : IJsonQueryable
{
    internal const string Keyword = "skip";

    [QueryParam(0)]
    public int Count { get; }

    public SkipQuery(int count)
    {
        Count = count;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        return new JsonArray(array.Skip(Count).Select(item => item?.DeepClone()).ToArray());
    }
}