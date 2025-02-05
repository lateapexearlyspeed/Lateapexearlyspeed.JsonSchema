using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(GetQueryParameterConverter<ExistsQuery>))]
[JsonQueryConverter(typeof(GetQueryParameterParserConverter<ExistsQuery>))]
public class ExistsQuery : IJsonQueryable, ISubGetQuery
{
    internal const string Keyword = "exists";

    public ExistsQuery(GetQuery getQuery)
    {
        SubGetQuery = getQuery;
    }

    public GetQuery SubGetQuery { get; }

    public JsonNode Query(JsonNode? data)
    {
        bool exist = SubGetQuery.QueryPropertyNameAndValue(data).exist;

        return JsonValue.Create(exist);
    }
}