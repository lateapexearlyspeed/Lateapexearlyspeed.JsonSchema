using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class ZipQuery : IJsonQueryable
{
    internal const string Keyword = "zip";

    [QueryArgument(0)]
    public IJsonQueryable FirstArrayQuery { get; }

    [QueryArgument(1)] 
    public IJsonQueryable SecondArrayQuery { get; }

    [QueryArgument(2)] 
    public IJsonQueryable ResultSelectorQuery { get; }

    public ZipQuery(IJsonQueryable firstArrayQuery, IJsonQueryable secondArrayQuery, IJsonQueryable resultSelectorQuery)
    {
        FirstArrayQuery = firstArrayQuery;
        SecondArrayQuery = secondArrayQuery;
        ResultSelectorQuery = resultSelectorQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (FirstArrayQuery.Query(data) is not JsonArray firstArray)
        {
            return null;
        }

        if (SecondArrayQuery.Query(data) is not JsonArray secondArray)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = firstArray.Zip(secondArray, (firstItem, secondItem) =>
        {
            var array = new JsonArray(firstItem?.DeepClone(), secondItem?.DeepClone());
            return ResultSelectorQuery.Query(array);
        });

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}