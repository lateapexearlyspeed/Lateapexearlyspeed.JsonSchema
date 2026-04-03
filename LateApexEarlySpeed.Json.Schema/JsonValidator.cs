using System.Diagnostics.Contracts;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema;

/// <summary>
/// class to provide functionality of json schema validation
/// </summary>
public class JsonValidator
{
    private static readonly HttpJsonDocumentClient HttpJsonDocumentClient = new();

    private readonly IJsonSchemaDocument _mainSchemaDoc;
    private IExternalSchemaDocumentPopulator? _externalSchemaDocumentPopulator;

    internal JsonValidatorOptions ValidatorOptions { get; }
    internal SchemaResourceRegistry GlobalSchemaResourceRegistry { get; } = new(1);

    /// <summary>
    /// Initializes a new instance of <see cref="JsonValidator"/> class for specified <paramref name="jsonSchema"/> and with specified <paramref name="options"/>
    /// </summary>
    /// <param name="jsonSchema">A json schema this <see cref="JsonValidator"/> represents</param>
    /// <param name="options">Options to control validation behavior</param>
    public JsonValidator(string jsonSchema, JsonValidatorOptions? options = null) : this(jsonSchema.AsSpan(), options)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="JsonValidator"/> class for specified <paramref name="jsonSchema"/> and with specified <paramref name="options"/>
    /// </summary>
    /// <param name="jsonSchema">A json schema this <see cref="JsonValidator"/> represents</param>
    /// <param name="options">Options to control validation behavior</param>
    public JsonValidator(ReadOnlySpan<char> jsonSchema, JsonValidatorOptions? options = null)
    {
        ValidatorOptions = InitializeJsonValidatorOptions(options);

        _mainSchemaDoc = JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(jsonSchema, GlobalSchemaResourceRegistry, ValidatorOptions);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="JsonValidator"/> class for specified <paramref name="jsonSchema"/> and with specified <paramref name="options"/>
    /// </summary>
    /// <param name="jsonSchema">A json schema this <see cref="JsonValidator"/> represents</param>
    /// <param name="options">Options to control validation behavior</param>
    public JsonValidator(JsonElement jsonSchema, JsonValidatorOptions? options = null)
    {
        ValidatorOptions = InitializeJsonValidatorOptions(options);

        _mainSchemaDoc = JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(jsonSchema, GlobalSchemaResourceRegistry, ValidatorOptions);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="JsonValidator"/> class for specified <paramref name="utf8JsonSchema"/> and with specified <paramref name="options"/>
    /// </summary>
    /// <param name="utf8JsonSchema">A json schema this <see cref="JsonValidator"/> represents</param>
    /// <param name="options">Options to control validation behavior</param>
    /// <remarks>This method will not dispose <paramref name="utf8JsonSchema"/> parameter </remarks>
    public JsonValidator(Stream utf8JsonSchema, JsonValidatorOptions? options = null)
    {
        ValidatorOptions = InitializeJsonValidatorOptions(options);

        _mainSchemaDoc = JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(utf8JsonSchema, GlobalSchemaResourceRegistry, ValidatorOptions);
    }

    internal JsonValidator(BodyJsonSchemaDocument mainSchemaDoc, JsonValidatorOptions? options = null)
    {
        ValidatorOptions = InitializeJsonValidatorOptions(options);

        JsonSchemaDocument.UpdateDocWithGlobalResourceRegistry(mainSchemaDoc, GlobalSchemaResourceRegistry);
        _mainSchemaDoc = mainSchemaDoc;
    }

    [Pure]
    private static JsonValidatorOptions InitializeJsonValidatorOptions(JsonValidatorOptions? options)
    {
        return options is null || options.Equals(JsonValidatorOptions.Default) 
            ? JsonValidatorOptions.Default 
            : options;
    }

    /// <summary>
    /// Add external json schema document
    /// </summary>
    /// <param name="externalJsonSchema">The content of external json schema document</param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void AddExternalDocument(string externalJsonSchema, JsonValidatorOptions? options = null) => AddExternalDocument(externalJsonSchema.AsSpan(), options);

    /// <summary>
    /// Add external json schema document
    /// </summary>
    /// <param name="externalJsonSchema">The content of external json schema document</param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void AddExternalDocument(ReadOnlySpan<char> externalJsonSchema, JsonValidatorOptions? options = null)
    {
        new ExternalSchemaRegistry(GlobalSchemaResourceRegistry, ValidatorOptions).Register(externalJsonSchema, options);
    }

    /// <summary>
    /// Add external json schema document
    /// </summary>
    /// <param name="externalUtf8JsonSchema">The content of external json schema document</param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void AddExternalDocument(Stream externalUtf8JsonSchema, JsonValidatorOptions? options = null)
    {
        new ExternalSchemaRegistry(GlobalSchemaResourceRegistry, ValidatorOptions).Register(externalUtf8JsonSchema, options);
    }

    /// <summary>
    /// Add external json schema document
    /// </summary>
    /// <param name="externalJsonSchema">The content of external json schema document</param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void AddExternalDocument(JsonElement externalJsonSchema, JsonValidatorOptions? options = null)
    {
        new ExternalSchemaRegistry(GlobalSchemaResourceRegistry, ValidatorOptions).Register(externalJsonSchema, options);
    }

    /// <summary>
    /// Add external json schema document which needs to be accessed by sending HTTP request
    /// </summary>
    /// <param name="remoteUri">The <see cref="Uri"/> of document the HTTP request is sent to access</param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    /// <returns></returns>
    public async Task AddHttpDocumentAsync(Uri remoteUri, JsonValidatorOptions? options = null)
    {
        string jsonSchemaText = await HttpJsonDocumentClient.GetDocumentAsync(remoteUri);
        AddExternalDocument(jsonSchemaText, options);
    }

    /// <summary>
    /// Gets or sets an <see cref="IExternalSchemaDocumentPopulator"/> instance for current <see cref="JsonValidator"/>, used to populate required external json schema document during validation (on-fly), compared with pre-registered external json schema document before validation by <see cref="AddExternalDocument(string, JsonValidatorOptions?)"/>
    /// </summary>
    /// <remarks>
    /// This <see cref="ExternalSchemaDocumentPopulator"/> will only populate external json schema document when there was no matched pre-registered external json schema document.
    /// </remarks>
    public IExternalSchemaDocumentPopulator? ExternalSchemaDocumentPopulator
    {
        get => _externalSchemaDocumentPopulator;
        set
        {
            _externalSchemaDocumentPopulator = value;
            GlobalSchemaResourceRegistry.EnableConcurrency = value is not null;
        }
    }

    /// <summary>
    /// Validate specified <paramref name="jsonInstance"/>
    /// </summary>
    /// <param name="jsonInstance">JSON instance to be validated</param>
    /// <param name="options">Options to control the validation behavior</param>
    /// <returns><see cref="ValidationResult"/> to represent validation result</returns>
    public ValidationResult Validate(string jsonInstance, JsonSchemaOptions? options = null)
    {
#pragma warning disable IDE0063
        using (JsonDocument instance = JsonDocument.Parse(jsonInstance))
#pragma warning restore IDE0063
        {
            return Validate(instance, options);
        }
    }

    /// <summary>
    /// Validate specified <paramref name="utf8JsonInstance"/>
    /// </summary>
    /// <param name="utf8JsonInstance">JSON instance to be validated</param>
    /// <param name="options">Options to control the validation behavior</param>
    /// <returns><see cref="ValidationResult"/> to represent validation result</returns>
    /// <remarks>This method will not dispose <paramref name="utf8JsonInstance"/> parameter </remarks>
    public ValidationResult Validate(Stream utf8JsonInstance, JsonSchemaOptions? options = null)
    {
#pragma warning disable IDE0063
        using (JsonDocument instance = JsonDocument.Parse(utf8JsonInstance))
#pragma warning restore IDE0063
        {
            return Validate(instance, options);
        }
    }

    /// <summary>
    /// Validate specified <paramref name="jsonInstance"/>
    /// </summary>
    /// <param name="jsonInstance">JSON instance to be validated</param>
    /// <param name="options">Options to control the validation behavior</param>
    /// <returns><see cref="ValidationResult"/> to represent validation result</returns>
    /// <remarks>This method will not dispose <paramref name="jsonInstance"/> parameter </remarks>
    public ValidationResult Validate(JsonDocument jsonInstance, JsonSchemaOptions? options = null)
    {
        return Validate(jsonInstance.RootElement, options);
    }

    /// <summary>
    /// Validate specified <paramref name="jsonInstance"/>
    /// </summary>
    /// <param name="jsonInstance">JSON instance to be validated</param>
    /// <param name="options">Options to control the validation behavior</param>
    /// <returns><see cref="ValidationResult"/> to represent validation result</returns>
    public ValidationResult Validate(JsonElement jsonInstance, JsonSchemaOptions? options = null)
    {
        options = new JsonSchemaOptions(options, this);

        return _mainSchemaDoc.DoValidation(new JsonInstanceElement(jsonInstance, LinkedListBasedImmutableJsonPointer.Empty), options);
    }

    /// <summary>
    /// Generate standard json schema text from its underlying main json schema document
    /// </summary>
    /// <returns>The json schema text constructed from standard json schema spec keywords (version 2020.12)</returns>
    /// <remarks>
    /// If there is extending keyword type in <see cref="JsonValidator"/> (for example constructing <see cref="JsonValidator"/> by some custom builder methods or attributes),
    /// this method will throw exception because extending keyword is out of scope of standard json schema spec.
    /// </remarks>
    /// <exception cref="NotSupportedException">There is extending keyword type in <see cref="JsonValidator"/> (for example constructing <see cref="JsonValidator"/> by some custom builder methods or attributes)</exception>
    public string GetStandardJsonSchemaText(JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(_mainSchemaDoc, options);
    }

    /// <summary>
    /// Gets or sets the maximum number of entries in the static cache of regular expressions for JSON Schema
    /// </summary>
    public static int GlobalRegexCacheSize
    {
        get => RegexMatcher.GlobalRegexProvider.CacheSize;
        set => RegexMatcher.GlobalRegexProvider.CacheSize = value;
    }

    /// <summary>
    /// Gets or sets an absolute expiration time for remote http json schema document cache, relative to cache creation time.
    /// Default value is <see cref="LateApexEarlySpeed.Json.Schema.Common.HttpJsonDocumentClient.DefaultCacheExpirationHours"/>.
    /// </summary>
    public static TimeSpan HttpDocumentCacheAbsoluteExpiration
    {
        get => HttpJsonDocumentClient.CacheAbsoluteExpirationTime;
        set => HttpJsonDocumentClient.CacheAbsoluteExpirationTime = value;
    }
}