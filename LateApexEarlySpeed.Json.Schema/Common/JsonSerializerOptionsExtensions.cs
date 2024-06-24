using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class JsonSerializerOptionsExtensions
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonValidatorOptions> JsonValidatorOptionTable = new();

    public static JsonValidatorOptions GetJsonValidatorOptions(this JsonSerializerOptions options)
    {
        bool canFind = JsonValidatorOptionTable.TryGetValue(options, out JsonValidatorOptions? jsonValidatorOptions);
        
        Debug.Assert(canFind);
        return jsonValidatorOptions;
    }

    public static void AddJsonValidatorOptions(this JsonSerializerOptions jsonSerializerOptions, JsonValidatorOptions jsonValidatorOptions)
    {
        JsonValidatorOptionTable.Add(jsonSerializerOptions, jsonValidatorOptions);
    }
}