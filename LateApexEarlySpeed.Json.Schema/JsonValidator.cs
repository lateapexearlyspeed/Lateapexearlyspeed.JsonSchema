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
    private readonly SchemaResourceRegistry _globalSchemaResourceRegistry = new(1);
    private readonly JsonValidatorOptions _jsonValidatorOptions;

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
        _jsonValidatorOptions = InitializeJsonValidatorOptions(options);

        _mainSchemaDoc = JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(jsonSchema, _globalSchemaResourceRegistry, _jsonValidatorOptions);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="JsonValidator"/> class for specified <paramref name="utf8JsonSchema"/> and with specified <paramref name="options"/>
    /// </summary>
    /// <param name="utf8JsonSchema">A json schema this <see cref="JsonValidator"/> represents</param>
    /// <param name="options">Options to control validation behavior</param>
    /// <remarks>This method will not dispose <paramref name="utf8JsonSchema"/> parameter </remarks>
    public JsonValidator(Stream utf8JsonSchema, JsonValidatorOptions? options = null)
    {
        _jsonValidatorOptions = InitializeJsonValidatorOptions(options);

        _mainSchemaDoc = JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(utf8JsonSchema, _globalSchemaResourceRegistry, _jsonValidatorOptions);
    }

    internal JsonValidator(BodyJsonSchemaDocument mainSchemaDoc, JsonValidatorOptions? options = null)
    {
        _jsonValidatorOptions = InitializeJsonValidatorOptions(options);

        JsonSchemaDocument.UpdateDocWithGlobalResourceRegistry(mainSchemaDoc, _globalSchemaResourceRegistry);
        _mainSchemaDoc = mainSchemaDoc;
    }

    [Pure]
    private static JsonValidatorOptions InitializeJsonValidatorOptions(JsonValidatorOptions? options)
    {
        return options is null || options.Equals(JsonValidatorOptions.Default) 
            ? JsonValidatorOptions.Default 
            : options;
    }

    [Pure]
    private JsonValidatorOptions CreateOverriddenJsonValidatorOptions(JsonValidatorOptions? options)
    {
        if (options is null)
        {
            return _jsonValidatorOptions;
        }

        return options.Equals(JsonValidatorOptions.Default) ? JsonValidatorOptions.Default : options;
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
        JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(externalJsonSchema, _globalSchemaResourceRegistry, CreateOverriddenJsonValidatorOptions(options));
    }

    /// <summary>
    /// Add external json schema document
    /// </summary>
    /// <param name="externalUtf8JsonSchema">The content of external json schema document</param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void AddExternalDocument(Stream externalUtf8JsonSchema, JsonValidatorOptions? options = null)
    {
        JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(externalUtf8JsonSchema, _globalSchemaResourceRegistry, CreateOverriddenJsonValidatorOptions(options));
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
        options = new JsonSchemaOptions(options, _globalSchemaResourceRegistry);

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