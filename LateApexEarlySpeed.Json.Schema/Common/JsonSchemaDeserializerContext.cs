using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Common;

/// <summary>
/// This is a workaround to associate <see cref="JsonValidatorOptions"/> instance info to <see cref="JsonSerializerOptions"/> instance,
/// by using a dedicated marker converter in options to carry deserialization context.
/// Note: for way of using <see cref="ConditionalWeakTable{TKey,TValue}"/>, it cannot always work, for example when: https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues/43.
/// Reason is: when running in that environment, <see cref="JsonSerializerOptions"/> instance may be changed to another instance by STJ between JsonSerializer.Deserialize(option) and custom JsonConverter.Read(option)
/// </summary>
internal ref struct JsonSchemaDeserializerContext
{
    // Items in JsonSerializerOptionsCache are with following order:
    //
    // | Dialect              | PropertyNameCaseInsensitive | Cache index |
    // |----------------------|-----------------------------|-------------|
    // | DialectKind.Draft2020| false                       | 0           |
    // | DialectKind.Draft2020| true                        | 1           |
    // | DialectKind.Draft2019| false                       | 2           |
    // | DialectKind.Draft2019| true                        | 3           |
    // | DialectKind.Draft7   | false                       | 4           |
    // | DialectKind.Draft7   | true                        | 5           |
    private static readonly JsonSerializerOptions[] JsonSerializerOptionsCache;

    public bool PropertyNameCaseInsensitive;
    public DialectKind Dialect;

    static JsonSchemaDeserializerContext()
    {
        JsonSerializerOptionsCache = new JsonSerializerOptions[ValidationKeywordRegistry.SupportedDialectsCount * 2];

        for (int i = 0; i < JsonSerializerOptionsCache.Length; i++)
        {
            var markerConverter = new JsonSchemaDeserializerContextMarkerConverter(new JsonSchemaDeserializerContext(i % 2 == 1, (DialectKind)(i / 2)));
            JsonSerializerOptionsCache[i] = new JsonSerializerOptions { Converters = { markerConverter } };
        }
    }

    public JsonSchemaDeserializerContext(bool propertyNameCaseInsensitive, DialectKind dialect)
    {
        PropertyNameCaseInsensitive = propertyNameCaseInsensitive;
        Dialect = dialect;
    }

    public JsonSchemaDeserializerContext(JsonSerializerOptions jsonSerializerOptions)
    {
        JsonConverter converter = jsonSerializerOptions.GetConverter(typeof(JsonSchemaDeserializerContextMarkerConverter.Marker));
        Debug.Assert(converter is JsonSchemaDeserializerContextMarkerConverter);

        JsonSchemaDeserializerContextMarkerConverter contextContainerConverter = (JsonSchemaDeserializerContextMarkerConverter)converter;

        PropertyNameCaseInsensitive = contextContainerConverter.PropertyNameCaseInsensitive;
        Dialect = contextContainerConverter.Dialect;
    }

    public readonly JsonSerializerOptions ToJsonSerializerOptions()
    {
        return JsonSerializerOptionsCache[(int)Dialect * 2 + (PropertyNameCaseInsensitive ? 1 : 0)];
    }
}

internal class JsonSchemaDeserializerContextMarkerConverter : JsonConverter<JsonSchemaDeserializerContextMarkerConverter.Marker>
{
    public JsonSchemaDeserializerContextMarkerConverter(JsonSchemaDeserializerContext jsonSchemaDeserializerContext)
    {
        PropertyNameCaseInsensitive = jsonSchemaDeserializerContext.PropertyNameCaseInsensitive;
        Dialect = jsonSchemaDeserializerContext.Dialect;
    }

    public bool PropertyNameCaseInsensitive { get; }
    public DialectKind Dialect { get; }

    public override Marker Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Marker converter is not intended for JSON payload read.");
    }

    public override void Write(Utf8JsonWriter writer, Marker value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Marker converter is not intended for JSON payload write.");
    }

    internal class Marker
    {
    }
}
