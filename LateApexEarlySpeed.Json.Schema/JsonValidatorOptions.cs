using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema;

public class JsonValidatorOptions
{
    /// <summary>
    /// See xml doc of <see cref="GlobalKeywordRegistry"/>
    /// </summary>
    private ValidationKeywordRegistry _globalKeywordRegistry = ValidationKeywordRegistry.Global;

    /// <summary>
    /// See xml doc of <see cref="GlobalFormatRegistry"/>
    /// </summary>
    private FormatRegistry _globalFormatRegistry = FormatRegistry.Global;

    /// <summary>
    /// It represents the global <see cref="ValidationKeywordRegistry"/> instance.
    /// The fallback global keyword registry used when the option-level registry <see cref="KeywordRegistry"/> does not contain an implementation for the requested keyword name and dialect.
    /// Defaults to ValidationKeywordRegistry.Global.
    /// </summary>
    /// <remarks>
    /// In test environment of this library, it can be assigned to be "per instance" rather than global level <see cref="ValidationKeywordRegistry.Global"/>
    /// so that each test case can have its own separated "global" keyword registry to read and write parallel without affecting other test cases (non-shared state).
    /// By now, the <see cref="ValidationKeywordRegistry.Global"/> is not thread-safe considering it should be modified during configuration time only.
    /// </remarks>
    internal ValidationKeywordRegistry GlobalKeywordRegistry
    {
        get => _globalKeywordRegistry;
        set
        {
            _globalKeywordRegistry = value;

            JsonSerializerOptionsCache ??= new JsonSerializerOptionsCache(this);
        }
    }

    /// <summary>
    /// It represents the global <see cref="FormatRegistry"/> instance.
    /// The fallback global format registry used when the option-level registry <see cref="FormatRegistry"/> does not contain an implementation for the requested format name.
    /// Defaults to FormatRegistry.Global.
    /// </summary>
    /// <remarks>
    /// In test environment of this library, it can be assigned to be "per instance" rather than global level <see cref="FormatRegistry.Global"/>
    /// so that each test case can have its own separated "global" format registry to read and write parallel without affecting other test cases (non-shared state).
    /// By now, the <see cref="FormatRegistry.Global"/> is not thread-safe considering it should be modified during configuration time only.
    /// </remarks>
    internal FormatRegistry GlobalFormatRegistry
    {
        get => _globalFormatRegistry;
        set
        {
            _globalFormatRegistry = value;

            JsonSerializerOptionsCache ??= new JsonSerializerOptionsCache(this);
        }
    }

    /// <summary>
    /// See doc of <see cref="KeywordRegistry"/> for details.
    /// </summary>
    internal ValidationKeywordRegistry? InternalKeywordRegistry { get; private set; }

    /// <summary>
    /// See doc of <see cref="FormatRegistry"/> property for details.
    /// </summary>
    internal FormatRegistry? InternalFormatRegistry { get; private set; }

    /// <summary>
    /// Non-null when this options instance must be carried by schema-deserialization JsonSerializerOptions,
    /// such as when it has a custom fallback global keyword registry (which is for test purpose) or an option-level keyword registry.
    /// Caches schema-deserialization <see cref="JsonSerializerOptions"/> instances per dialect for this options instance.
    /// </summary>
    internal JsonSerializerOptionsCache? JsonSerializerOptionsCache { get; private set; }

    /// <summary>
    /// Gets or sets a value that determines whether a property's name uses a case-insensitive comparison during validation. The default value is false.
    /// </summary>
    /// <returns>
    /// true to compare property names using case-insensitive comparison; otherwise, false.
    /// </returns>
    public bool PropertyNameCaseInsensitive { get; set; }

    /// <summary>
    /// Gets or sets a value that determines whether removing json schema resource id from unknown keywords. The default value is false.
    /// </summary>
    public bool IgnoreResourceIdInUnknownKeyword { set; get; }

    /// <summary>
    /// Gets or sets a value that determines whether to collect annotation values during validation. The default value is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// The annotation collection happens only when <see cref="CollectAnnotation"/> is <see langword="true"/>, the validation result is valid and <see cref="JsonSchemaOptions.OutputFormat"/> is <see cref="OutputFormat.List"/>
    /// </remarks>
    public bool CollectAnnotation { get; set; }

    /// <summary>
    /// Gets or sets a value that determines default dialect when there is no '$schema' identifier in Json schema. The default value is <see cref="DialectKind.Draft202012"/>.
    /// </summary>
    public DialectKind DefaultDialect { get; set; }

    /// <summary>
    /// Gets the <see cref="ValidationKeywordRegistry"/> instance that registers and retrieves validation keywords.
    /// </summary>
    /// <remarks>
    /// A keyword implementation is resolved per keyword name and dialect. This per <see cref="JsonValidatorOptions"/> level <see cref="ValidationKeywordRegistry"/> takes higher precedence than <see cref="GlobalKeywordRegistry"/>: the global registry is only consulted when this registry has no implementation registered for that specific keyword name and dialect (even if the same keyword name is registered here for other dialects).
    /// </remarks>
    public ValidationKeywordRegistry KeywordRegistry
    {
        get
        {
            if (InternalKeywordRegistry is null)
            {
                InternalKeywordRegistry = new ValidationKeywordRegistry(2, false);

                JsonSerializerOptionsCache ??= new JsonSerializerOptionsCache(this);
            }

            return InternalKeywordRegistry;
        }
    }

    /// <summary>
    /// Gets the <see cref="LateApexEarlySpeed.Json.Schema.Keywords.FormatRegistry"/> instance that registers and retrieves format validators.
    /// </summary>
    /// <remarks>
    /// A format validator implementation is resolved per format name. This per <see cref="JsonValidatorOptions"/> level <see cref="FormatRegistry"/> takes higher precedence than <see cref="GlobalFormatRegistry"/>: the global registry is only consulted when this registry has no implementation registered for that specific format name.
    /// </remarks>
    public FormatRegistry FormatRegistry
    {
        get
        {
            if (InternalFormatRegistry is null)
            {
                InternalFormatRegistry = new FormatRegistry(false);

                JsonSerializerOptionsCache ??= new JsonSerializerOptionsCache(this);
            }

            return InternalFormatRegistry;
        }
    }

    internal static JsonValidatorOptions Default { get; } = new();

    internal bool Equals(JsonValidatorOptions other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return PropertyNameCaseInsensitive == other.PropertyNameCaseInsensitive 
               && IgnoreResourceIdInUnknownKeyword == other.IgnoreResourceIdInUnknownKeyword
               && CollectAnnotation == other.CollectAnnotation
               && DefaultDialect == other.DefaultDialect
               && ReferenceEquals(InternalKeywordRegistry, other.InternalKeywordRegistry)
               && ReferenceEquals(GlobalKeywordRegistry, other.GlobalKeywordRegistry)
               && ReferenceEquals(InternalFormatRegistry, other.InternalFormatRegistry)
               && ReferenceEquals(GlobalFormatRegistry, other.GlobalFormatRegistry);
    }
}

/// <summary>
/// Caches schema-deserialization <see cref="JsonSerializerOptions"/> instances per dialect
/// for a single <see cref="JsonValidatorOptions"/> instance.
/// </summary>
internal class JsonSerializerOptionsCache
{
    /// <summary>
    /// The validator options that owns this cache.
    /// </summary>
    private readonly JsonValidatorOptions _parent;

    private readonly JsonSerializerOptions?[] _jsonSerializerOptionsCache = new JsonSerializerOptions[ValidationKeywordRegistry.SupportedDialectsCount];

    public JsonSerializerOptionsCache(JsonValidatorOptions parent)
    {
        _parent = parent;
    }

    public JsonSerializerOptions GetJsonSerializerOptions(DialectKind dialect)
    {
        return _jsonSerializerOptionsCache[(int)dialect] ??= new JsonSerializerOptions
        {
            Converters = { new JsonSchemaDeserializerContextMarkerConverter(_parent, dialect) }
        };
    }
}
