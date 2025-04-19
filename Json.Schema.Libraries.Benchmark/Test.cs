using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Libraries.Benchmark;

/// <summary>
/// Refer to: https://github.com/json-schema-org/JSON-Schema-Test-Suite#terminology
/// </summary>
internal class Test
{
    [JsonPropertyName("data")]
    public JsonElement InstanceElement
    {
        set => Instance = JsonSerializer.Serialize(value);
    }

    [JsonIgnore]
    public string Instance { get; private set; } = null!;

    [JsonPropertyName("valid")]
    public bool ValidationResult { get; set; }

    public TestValidationResult ToTestValidationResult => ValidationResult ? TestValidationResult.Positive : TestValidationResult.Negative;
}