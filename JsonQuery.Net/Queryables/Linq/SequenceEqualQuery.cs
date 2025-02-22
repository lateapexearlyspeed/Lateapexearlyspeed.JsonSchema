using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class SequenceEqualQuery : IJsonQueryable
{
    internal const string Keyword = "sequenceEqual";

    public IJsonQueryable FirstSequenceQuery { get; }
    public IJsonQueryable SecondSequenceQuery { get; }

    public SequenceEqualQuery(IJsonQueryable firstSequenceQuery, IJsonQueryable secondSequenceQuery)
    {
        FirstSequenceQuery = firstSequenceQuery;
        SecondSequenceQuery = secondSequenceQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (FirstSequenceQuery.Query(data) is not JsonArray firstArray)
        {
            return null;
        }

        if (SecondSequenceQuery.Query(data) is not JsonArray secondArray)
        {
            return null;
        }

        return JsonNode.DeepEquals(firstArray, secondArray);
    }
}