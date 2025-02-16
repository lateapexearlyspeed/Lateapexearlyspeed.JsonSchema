using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter<NotQuery>))]
public class NotQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "not";

    public NotQuery(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public IJsonQueryable SubQuery { get; }

    public JsonNode Query(JsonNode? data)
    {
        bool queryResult = SubQuery.Query(data).GetBooleanValue();

        return JsonValue.Create(!queryResult);
    }
}