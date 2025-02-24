using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class JoinArraysQuery : IJsonQueryable
{
    internal const string Keyword = "joinLinq";

    [QueryParam(0)]
    public IJsonQueryable OuterArrayQuery { get; }

    [QueryParam(1)] 
    public IJsonQueryable InnerArrayQuery { get; }

    [QueryParam(2)] 
    public IJsonQueryable OuterKeySelector { get; }

    [QueryParam(3)] 
    public IJsonQueryable InnerKeySelector { get; }

    [QueryParam(4)] 
    public IJsonQueryable ResultSelector { get; }

    public JoinArraysQuery(IJsonQueryable outerArrayQuery, IJsonQueryable innerArrayQuery, IJsonQueryable outerKeySelector, IJsonQueryable innerKeySelector, IJsonQueryable resultSelector)
    {
        OuterArrayQuery = outerArrayQuery;
        InnerArrayQuery = innerArrayQuery;
        OuterKeySelector = outerKeySelector;
        InnerKeySelector = innerKeySelector;
        ResultSelector = resultSelector;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (OuterArrayQuery.Query(data) is not JsonArray outerArray)
        {
            return null;
        }

        if (InnerArrayQuery.Query(data) is not JsonArray innerArray)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = outerArray.Join(innerArray, 
            outerItem => OuterKeySelector.Query(outerItem), 
            innerItem => InnerKeySelector.Query(innerItem), 
            (outer, inner) => ResultSelector.Query(new JsonArray(outer?.DeepClone(), inner?.DeepClone())), new JsonNodeEqualityComparer());

        return new JsonArray(result.Select(item => item?.DeepClone()).ToArray());
    }
}