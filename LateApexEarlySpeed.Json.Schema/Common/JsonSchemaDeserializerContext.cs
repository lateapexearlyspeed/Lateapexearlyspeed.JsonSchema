using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// This is a workaround about associate <see cref="JsonValidatorOptions"/> instance info to <see cref="JsonSerializerOptions"/> instance, which currently is reliable way to implement.
/// Note: for way of using <see cref="ConditionalWeakTable{TKey,TValue}"/>, it cannot always work, for example when: https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues/43.
/// Reason is: when running in that environment, <see cref="JsonSerializerOptions"/> instance may be changed to another instance by STJ between JsonSerializer.Deserialize(option) and custom JsonConverter.Read(option)
///
/// TODO: when STJ releases new interface to accept state, this implementation should be updated (or removed).
/// </summary>
internal ref struct JsonSchemaDeserializerContext
{
    // Default JsonSerializerOptions.MaxDepth value is 0 (which represents 64), so here uses 65 to represent default validator option and 66 to represent enabling PropertyNameCaseInsensitive validation, etc. It is a workaround and not graceful, I know…
    private const int BaseMaxDepth = 65;

    // Items in JsonSerializerOptionsCache are with following order:
    //
    // | Dialect              | PropertyNameCaseInsensitive | MaxDepth |
    // |----------------------|-----------------------------|----------|
    // | DialectKind.Draft2020| false                       | 65       |
    // | DialectKind.Draft2020| true                        | 66       |
    // | DialectKind.Draft2019| false                       | 67       |
    // | DialectKind.Draft2019| true                        | 68       |
    // | DialectKind.Draft7   | false                       | 69       |
    // | DialectKind.Draft7   | true                        | 70       |
    private static readonly JsonSerializerOptions[] JsonSerializerOptionsCache;

    public bool PropertyNameCaseInsensitive;
    public DialectKind Dialect;

    static JsonSchemaDeserializerContext()
    {
        JsonSerializerOptionsCache = new JsonSerializerOptions[ValidationKeywordRegistry.SupportedDialectsCount * 2];

        for (int i = 0; i < JsonSerializerOptionsCache.Length; i++)
        {
            JsonSerializerOptionsCache[i] = new JsonSerializerOptions { MaxDepth = BaseMaxDepth + i };
        }
    }

    public JsonSchemaDeserializerContext(bool propertyNameCaseInsensitive, DialectKind dialect)
    {
        PropertyNameCaseInsensitive = propertyNameCaseInsensitive;
        Dialect = dialect;
    }

    public JsonSchemaDeserializerContext(JsonSerializerOptions jsonSerializerOptions)
    {
        int maxDepthIdx = jsonSerializerOptions.MaxDepth - BaseMaxDepth;
        Debug.Assert(maxDepthIdx >= 0 && maxDepthIdx < ValidationKeywordRegistry.SupportedDialectsCount * 2);

        PropertyNameCaseInsensitive = (maxDepthIdx & 0x1) == 0x1; // maxDepthIdx % 2 == 1;
        Dialect = (DialectKind)(maxDepthIdx >> 2); // maxDepthIdx / 2
    }

    public readonly JsonSerializerOptions ToJsonSerializerOptions()
    {
        return JsonSerializerOptionsCache[(int)Dialect * 2 + (PropertyNameCaseInsensitive ? 1 : 0)];
    }
}