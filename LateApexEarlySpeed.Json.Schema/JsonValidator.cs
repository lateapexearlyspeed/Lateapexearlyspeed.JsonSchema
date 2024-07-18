using System.Diagnostics.Contracts;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LateApexEarlySpeed.Json.Schema;

public class JsonValidator
{
    private const string HttpClientName = "lateapexearlyspeed";
    private static readonly IHttpClientFactory HttpClientFactory;

    private readonly IJsonSchemaDocument _mainSchemaDoc;
    private readonly SchemaResourceRegistry _globalSchemaResourceRegistry = new();
    private readonly JsonValidatorOptions _jsonValidatorOptions;

    static JsonValidator()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(HttpClientName);
        ServiceProvider sp = services.BuildServiceProvider();
        HttpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    }

    public JsonValidator(string jsonSchema, JsonValidatorOptions? options = null) : this(jsonSchema.AsSpan(), options)
    {
    }

    public JsonValidator(ReadOnlySpan<char> jsonSchema, JsonValidatorOptions? options = null)
    {
        _jsonValidatorOptions = InitializeJsonValidatorOptions(options);

        _mainSchemaDoc = JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(jsonSchema, _globalSchemaResourceRegistry, _jsonValidatorOptions);
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

    public void AddExternalDocument(string externalJsonSchema) => AddExternalDocument(externalJsonSchema.AsSpan());

    public void AddExternalDocument(ReadOnlySpan<char> externalJsonSchema)
    {
        JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(externalJsonSchema, _globalSchemaResourceRegistry, _jsonValidatorOptions);
    }

    public async Task AddHttpDocumentAsync(Uri remoteUri)
    {
        HttpClient httpClient = HttpClientFactory.CreateClient(HttpClientName);
        HttpResponseMessage response = await httpClient.GetAsync(remoteUri);
        
        response.EnsureSuccessStatusCode();

        string jsonSchemaText = await response.Content.ReadAsStringAsync();
        AddExternalDocument(jsonSchemaText);
    }

    public ValidationResult Validate(string jsonInstance, JsonSchemaOptions? options = null)
    {
        // ReSharper disable once ConvertToUsingDeclaration
        using (JsonDocument instance = JsonDocument.Parse(jsonInstance))
        {
            options = new JsonSchemaOptions(options, _globalSchemaResourceRegistry);

            return _mainSchemaDoc.DoValidation(instance.RootInstanceElement(), options);
        }
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

    public static int DefaultRegexCacheSize
    {
        set => RegexMatcher.DefaultRegexProvider.CacheSize = value;
    }

    public static int DefaultRegexCacheRemovalSelectSize
    {
        set => RegexMatcher.DefaultRegexProvider.RemovalSelectSize = value;
    }

    public static IRegexProvider RegexProvider
    {
        set => RegexMatcher.RegexProvider = value;
    }
}