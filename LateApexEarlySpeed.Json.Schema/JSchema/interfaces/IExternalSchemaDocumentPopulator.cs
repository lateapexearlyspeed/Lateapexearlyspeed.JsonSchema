namespace LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

/// <summary>
/// Determines the populator to populate and register external json schema document
/// </summary>
public interface IExternalSchemaDocumentPopulator
{
    /// <summary>
    /// Fetch external json schema document according to <paramref name="baseUri"/> and register it into <paramref name="externalSchemaRegistry"/>.
    /// </summary>
    /// <remarks>
    /// Pass <paramref name="baseUri"/> as the retrievalUri argument of <see cref="ExternalSchemaRegistry.Register(ReadOnlySpan{char}, Uri?, JsonValidatorOptions?)"/> (or its other overloads),
    /// otherwise the registered document can only be found when it has an absolute $id equal to <paramref name="baseUri"/>.
    /// </remarks>
    void Populate(Uri baseUri, ExternalSchemaRegistry externalSchemaRegistry);
}