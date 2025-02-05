using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(SingleQueryParameterConverter<NumberQuery>))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter<NumberQuery>))]
public class NumberQuery : IJsonQueryable, ISingleSubQuery
{
    internal const string Keyword = "number";

    public NumberQuery(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public IJsonQueryable SubQuery { get; }

    public JsonNode? Query(JsonNode? data)
    {
        JsonNode? stringNode = SubQuery.Query(data);
        if (stringNode is null || stringNode.GetValueKind() != JsonValueKind.String)
        {
            return null;
        }

        string numericString = stringNode.GetValue<string>();

        return decimal.TryParse(numericString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal numericValue)
            ? numericValue
            : null;
    }
}