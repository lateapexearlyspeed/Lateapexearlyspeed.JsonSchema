using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// This is a workaround about associate <see cref="JsonValidatorOptions"/> instance info to <see cref="JsonSerializerOptions"/> instance, which currently is reliable way to implement.
/// Note: for way of using <see cref="ConditionalWeakTable{TKey,TValue}"/>, it cannot always work, for example when: https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues/43.
/// Reason is: when running in that environment, <see cref="JsonSerializerOptions"/> instance may be changed to another instance by STJ between JsonSerializer.Deserialize(option) and custom JsonConverter.Read(option)
///
/// TODO: when STJ releases new interface to accept state, this implementation should be updated (or removed).
/// </summary>
internal static class JsonValidatorOptionsJsonSerializerOptionsMapper
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions;

    private static readonly JsonSerializerOptions PropertyNameCaseInsensitiveSerializerOptions;

    private static readonly JsonValidatorOptions PropertyNameCaseInsensitiveValidatorOptions;

    static JsonValidatorOptionsJsonSerializerOptionsMapper()
    {
        // Default MaxDepth value is 0 (which represents 64), so here uses 65 to represent default validator option and 66 to represent enabling PropertyNameCaseInsensitive validation. It is a workaround and not graceful, I know..
        DefaultSerializerOptions = new JsonSerializerOptions { MaxDepth = 65 };
        PropertyNameCaseInsensitiveSerializerOptions = new JsonSerializerOptions { MaxDepth = 66 };

        PropertyNameCaseInsensitiveValidatorOptions = new JsonValidatorOptions { PropertyNameCaseInsensitive = true };
    }

    public static JsonSerializerOptions ToJsonSerializerOptions(JsonValidatorOptions jsonValidatorOptions)
    {
        return jsonValidatorOptions.Equals(JsonValidatorOptions.Default)
            ? DefaultSerializerOptions
            : PropertyNameCaseInsensitiveSerializerOptions;
    }

    public static JsonValidatorOptions ToJsonValidatorOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        return jsonSerializerOptions.MaxDepth == 65
            ? JsonValidatorOptions.Default
            : PropertyNameCaseInsensitiveValidatorOptions;
    }
}