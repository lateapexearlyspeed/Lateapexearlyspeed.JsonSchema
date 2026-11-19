using System.Diagnostics.Contracts;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

public readonly struct ExternalSchemaRegistry
{
    private readonly SchemaResourceRegistry _globalSchemaResourceRegistry;
    private readonly JsonValidatorOptions _jsonValidatorOptions;

    internal ExternalSchemaRegistry(SchemaResourceRegistry globalSchemaResourceRegistry, JsonValidatorOptions jsonValidatorOptions)
    {
        _globalSchemaResourceRegistry = globalSchemaResourceRegistry;
        _jsonValidatorOptions = jsonValidatorOptions;
    }

    /// <summary>
    /// Register external json schema document
    /// </summary>
    /// <param name="schema">The content of external json schema document</param>
    /// <param name="retrievalUri">
    /// The URI from which <paramref name="schema"/> is retrieved. It must be an absolute URI without fragment.
    /// It is used as the initial base URI of the document: when the document has no $id, it is the base URI of the document; when the document has a relative $id, the $id is resolved against it.
    /// The document can also be referenced (e.g. by $ref) through this URI.
    /// When it is null, the document can only be referenced through its absolute $id.
    /// </param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void Register(ReadOnlySpan<char> schema, Uri? retrievalUri = null, JsonValidatorOptions? options = null)
    {
        JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(schema, _globalSchemaResourceRegistry, CreateOverriddenJsonValidatorOptions(options), retrievalUri);
    }

    /// <summary>
    /// Register external json schema document
    /// </summary>
    /// <param name="schema">The content of external json schema document</param>
    /// <param name="retrievalUri">
    /// The URI from which <paramref name="schema"/> is retrieved. It must be an absolute URI without fragment.
    /// It is used as the initial base URI of the document: when the document has no $id, it is the base URI of the document; when the document has a relative $id, the $id is resolved against it.
    /// The document can also be referenced (e.g. by $ref) through this URI.
    /// When it is null, the document can only be referenced through its absolute $id.
    /// </param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void Register(JsonElement schema, Uri? retrievalUri = null, JsonValidatorOptions? options = null)
    {
        JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(schema, _globalSchemaResourceRegistry, CreateOverriddenJsonValidatorOptions(options), retrievalUri);
    }

    /// <summary>
    /// Register external json schema document
    /// </summary>
    /// <param name="utf8Schema">The content of external json schema document</param>
    /// <param name="retrievalUri">
    /// The URI from which <paramref name="utf8Schema"/> is retrieved. It must be an absolute URI without fragment.
    /// It is used as the initial base URI of the document: when the document has no $id, it is the base URI of the document; when the document has a relative $id, the $id is resolved against it.
    /// The document can also be referenced (e.g. by $ref) through this URI.
    /// When it is null, the document can only be referenced through its absolute $id.
    /// </param>
    /// <param name="options">Options to control validation behavior for external schema document. Use option value for creating <see cref="JsonValidator"/> instance when it is null.</param>
    public void Register(Stream utf8Schema, Uri? retrievalUri = null, JsonValidatorOptions? options = null)
    {
        JsonSchemaDocument.CreateDocAndUpdateGlobalResourceRegistry(utf8Schema, _globalSchemaResourceRegistry, CreateOverriddenJsonValidatorOptions(options), retrievalUri);
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
}