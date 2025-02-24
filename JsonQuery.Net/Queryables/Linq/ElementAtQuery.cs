using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class ElementAtQuery : IJsonQueryable
{
    internal const string Keyword = "elementAt";

    [QueryArgument(0)]
    public int Index { get; }

    public ElementAtQuery(int index)
    {
        Index = index;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (data is not JsonArray array)
        {
            return null;
        }

        if (Index < 0 || Index >= array.Count)
        {
            return null;
        }

        return array[Index];
    }
}