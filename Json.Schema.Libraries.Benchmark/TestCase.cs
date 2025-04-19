using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Schema;
using JsonValidator = LateApexEarlySpeed.Json.Schema.JsonValidator;

namespace Json.Schema.Libraries.Benchmark;

/// <summary>
/// Refer to: https://github.com/json-schema-org/JSON-Schema-Test-Suite#terminology
/// </summary>
internal class TestCase
{
    [JsonPropertyName("schema")]
    public JsonElement JsonSchemaElement
    {
        set => JsonSchema = JsonSerializer.Serialize(value);
    }

    [JsonIgnore]
    public string JsonSchema { get; private set; } = null!;

    [JsonPropertyName("tests")]
    public Test[] Tests { get; set; } = null!;

    public string Description { get; set; } = null!;

    [JsonIgnore]
    public JsonValidator? LateApexEarlySpeedValidator { get; set; }

    [JsonIgnore]
    public NJsonSchema.JsonSchema? NJsonSchemaValidator { get; set; }

    [JsonIgnore]
    public JsonSchema? JsonSchemaDotNetValidator { get; set; }

    [JsonIgnore]
    public JSchema? NewtonsoftValidator { get; set; }
}