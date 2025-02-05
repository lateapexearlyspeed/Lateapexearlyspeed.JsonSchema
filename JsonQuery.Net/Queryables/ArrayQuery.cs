using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(QueryCollectionConverter<ArrayQuery>))]
public class ArrayQuery : IJsonQueryable, IMultipleSubQuery
{
    internal const string Keyword = "array";

    private readonly IJsonQueryable[] _queries;

    public ArrayQuery(IJsonQueryable[] queries)
    {
        _queries = queries;
    }

    public JsonNode Query(JsonNode? data)
    {
        return new JsonArray(_queries.Select(query => query.Query(data)?.DeepClone()).ToArray());
    }

    public IEnumerable<IJsonQueryable> SubQueries => _queries;
}