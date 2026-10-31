using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.JSchema;
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
    // | Dialect                 | PropertyNameCaseInsensitive | CollectAnnotations | Cache index |
    // |-------------------------|-----------------------------|--------------------|-------------|
    // | DialectKind.Draft202012 | false                       | false              | 0           |
    // | DialectKind.Draft202012 | false                       | true               | 1           |
    // | DialectKind.Draft202012 | true                        | false              | 2           |
    // | DialectKind.Draft202012 | true                        | true               | 3           |
    // | DialectKind.Draft201909 | false                       | false              | 4           |
    // | DialectKind.Draft201909 | false                       | true               | 5           |
    // | DialectKind.Draft201909 | true                        | false              | 6           |
    // | DialectKind.Draft201909 | true                        | true               | 7           |
    // | DialectKind.Draft7      | false                       | false              | 8           |
    // | DialectKind.Draft7      | false                       | true               | 9           |
    // | DialectKind.Draft7      | true                        | false              | 10          |
    // | DialectKind.Draft7      | true                        | true               | 11          |
    private static readonly JsonSerializerOptions[] JsonSerializerOptionsCache;

    /// <summary>
    /// The validator options associated with this deserialization context.
    /// Provides access to property-name comparison settings, annotation collection setting and keyword resolution.
    /// </summary>
    private readonly JsonValidatorOptions _jsonValidatorOptions;

    public DialectKind? Dialect;
    public Uri MetaSchemaId;
    public readonly bool PropertyNameCaseInsensitive => _jsonValidatorOptions.PropertyNameCaseInsensitive;
    public readonly bool CollectAnnotations => _jsonValidatorOptions.CollectAnnotations;
    public readonly MetaSchemaRegistry? InternalMetaSchemaRegistry => _jsonValidatorOptions.InternalCustomMetaSchemaRegistry;

    static JsonSchemaDeserializerContext()
    {
        JsonSerializerOptionsCache = new JsonSerializerOptions[ValidationKeywordRegistry.SupportedDialectsCount * 2 * 2];

        for (int i = 0; i < JsonSerializerOptionsCache.Length; i++)
        {
            DialectKind dialect = (DialectKind)(i / 4);
            var markerConverter = new JsonSchemaDeserializerContextMarkerConverter(new JsonValidatorOptions { PropertyNameCaseInsensitive = (i / 2) % 2 == 1, CollectAnnotations = i % 2 == 1 }, dialect, SchemaKeyword.GetDraftIdentifier(dialect));
            JsonSerializerOptionsCache[i] = new JsonSerializerOptions { Converters = { markerConverter } };
        }
    }

    public JsonSchemaDeserializerContext(JsonSerializerOptions jsonSerializerOptions)
    {
        JsonConverter converter = jsonSerializerOptions.GetConverter(typeof(JsonSchemaDeserializerContextMarkerConverter.Marker));
        Debug.Assert(converter is JsonSchemaDeserializerContextMarkerConverter);

        JsonSchemaDeserializerContextMarkerConverter contextContainerConverter = (JsonSchemaDeserializerContextMarkerConverter)converter;

        _jsonValidatorOptions = contextContainerConverter.Options;
        Dialect = contextContainerConverter.Dialect;
        MetaSchemaId = contextContainerConverter.MetaSchemaId;
    }

    public JsonSchemaDeserializerContext(JsonValidatorOptions jsonValidatorOptions, DialectKind dialect)
    {
        _jsonValidatorOptions = jsonValidatorOptions;
        Dialect = dialect;
        MetaSchemaId = SchemaKeyword.GetDraftIdentifier(dialect);
    }

    public bool IsCustomMetaSchema
    {
        get
        {
            if (_jsonValidatorOptions.InternalCustomMetaSchemaRegistry is null || SchemaKeyword.TryGetStandardMetaSchema(MetaSchemaId, out _))
            {
                return false;
            }

            return _jsonValidatorOptions.InternalCustomMetaSchemaRegistry.GetMetaSchema(MetaSchemaId) is not null;
        }
    }

    public readonly JsonSerializerOptions ToJsonSerializerOptions()
    {
        if (_jsonValidatorOptions.JsonSerializerOptionsCache is null)
        {
            Debug.Assert(Dialect.HasValue);
            return JsonSerializerOptionsCache[(int)Dialect.Value * 4 + (PropertyNameCaseInsensitive ? 2 : 0) + (CollectAnnotations ? 1 : 0)];
        }

        return _jsonValidatorOptions.JsonSerializerOptionsCache.GetJsonSerializerOptions(MetaSchemaId);
    }

    public Type? GetValidationKeyword(scoped ReadOnlySpan<char> keywordName)
    {
        Debug.Assert(Dialect.HasValue);

        return _jsonValidatorOptions.InternalKeywordRegistry?.GetKeyword(keywordName, Dialect.Value) ?? _jsonValidatorOptions.GlobalKeywordRegistry.GetKeyword(keywordName, Dialect.Value);
    }

    public FormatValidator? GetFormatValidator(string formatName)
    {
        return _jsonValidatorOptions.InternalFormatRegistry?.GetFormatValidator(formatName) ?? _jsonValidatorOptions.GlobalFormatRegistry.GetFormatValidator(formatName);
    }

    public Type? GetValidationKeywordFromVocabulary(scoped ReadOnlySpan<char> keywordName)
    {
        return GetCurrentCustomMetaSchema().GetKeyword(keywordName);
    }

    public bool ContainsKeywordInVocabulary(scoped ReadOnlySpan<char> keywordName)
    {
        return GetCurrentCustomMetaSchema().ContainsKeyword(keywordName);
    }

    private MetaSchema GetCurrentCustomMetaSchema()
    {
        Debug.Assert(IsCustomMetaSchema);
        Debug.Assert(_jsonValidatorOptions.InternalCustomMetaSchemaRegistry is not null);

        MetaSchema? metaSchema = _jsonValidatorOptions.InternalCustomMetaSchemaRegistry.GetMetaSchema(MetaSchemaId);
        Debug.Assert(metaSchema is not null);

        return metaSchema;
    }
}

internal class JsonSchemaDeserializerContextMarkerConverter : JsonConverter<JsonSchemaDeserializerContextMarkerConverter.Marker>
{
    private readonly DialectKind? _dialect;

    public JsonValidatorOptions Options { get; }

    public DialectKind? Dialect
    {
        get
        {
            if (_dialect.HasValue)
            {
                return _dialect.Value;
            }

            if (SchemaKeyword.TryGetDialectKind(MetaSchemaId, Options.InternalCustomMetaSchemaRegistry, out DialectKind dialect))
            {
                return dialect;
            }

            return null;
        }
    }

    public Uri MetaSchemaId { get; }

    public JsonSchemaDeserializerContextMarkerConverter(JsonValidatorOptions jsonValidatorOptions, DialectKind dialect, Uri metaSchemaId)
    {
        Options = jsonValidatorOptions;
        _dialect = dialect;
        MetaSchemaId = metaSchemaId;
    }

    public JsonSchemaDeserializerContextMarkerConverter(JsonValidatorOptions jsonValidatorOptions, Uri metaSchemaId)
    {
        Options = jsonValidatorOptions;
        MetaSchemaId = metaSchemaId;

        if (SchemaKeyword.TryGetStandardMetaSchema(metaSchemaId, out SchemaKeyword? schemaKeyword))
        {
            Debug.Assert(schemaKeyword.Dialect.HasValue);

            _dialect = schemaKeyword.Dialect;
        }
    }

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
