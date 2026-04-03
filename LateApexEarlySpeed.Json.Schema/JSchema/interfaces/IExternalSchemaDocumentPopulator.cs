namespace LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

/// <summary>
/// Determines the populator to populate and register external json schema document
/// </summary>
public interface IExternalSchemaDocumentPopulator
{
    /// <summary>
    /// Fetch external json schema document according to <paramref name="baseUri"/> and register it into <paramref name="externalSchemaRegistry"/>.
    /// </summary>
    void Populate(Uri baseUri, ExternalSchemaRegistry externalSchemaRegistry);
}