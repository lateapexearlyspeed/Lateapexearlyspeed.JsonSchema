using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter<StringQuery>))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter<StringQuery>))]
public class StringQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "string";

    public StringQuery(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public IJsonQueryable SubQuery { get; }

    public JsonNode Query(JsonNode? data)
    {
        JsonNode? node = SubQuery.Query(data);

        return node is null ? "null" : node.ToString();
    }
}