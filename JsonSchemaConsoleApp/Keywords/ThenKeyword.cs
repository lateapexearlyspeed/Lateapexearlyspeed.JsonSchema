using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

// [JsonConverter(typeof(SingleSchemaJsonConverter<ThenKeyword>))]
public class ThenKeyword
{
    public const string Keyword = "then";

    // public JsonSchema? Schema { get; set; }
}