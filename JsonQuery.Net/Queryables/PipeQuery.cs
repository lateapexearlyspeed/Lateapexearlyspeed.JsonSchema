using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(QueryCollectionConverter))]
[JsonQueryConverter(typeof(QueryCollectionParserConverter<PipeQuery>))]
public class PipeQuery : IJsonQueryable, IMultipleSubQuery
{
    internal const string Keyword = "pipe";

    private readonly IJsonQueryable[] _queries;

    public PipeQuery(IJsonQueryable[] queries)
    {
        _queries = queries;
    }

    public JsonNode? Query(JsonNode? data)
    {
        JsonNode? curNode = data;

        foreach (IJsonQueryable query in _queries)
        {
            curNode = query.Query(curNode);
        }

        return curNode;
    }

    public IEnumerable<IJsonQueryable> SubQueries => _queries;
}