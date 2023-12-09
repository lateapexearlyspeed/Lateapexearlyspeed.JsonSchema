using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BodyJsonSchemaDocument : JsonSchemaResource, IJsonSchemaDocument
{
    private static readonly Uri DefaultDocumentBaseUri = new("http://lateapexearlyspeed");

    /// <summary>
    /// All schema resources inside current document (including current document itself)
    /// </summary>
    public SchemaResourceRegistry LocalSchemaResourceRegistry { get; } = new();

    public SchemaResourceRegistry? GlobalSchemaResourceRegistry { get; set; }

    internal BodyJsonSchemaDocument(List<KeywordBase> keywords, List<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReferenceKeyword? schemaReference, SchemaDynamicReferenceKeyword? schemaDynamicReference, string? anchor, string? dynamicAnchor, Uri? id = null, DefsKeyword? defsKeyword = null)
        : base(GetBaseUri(id), keywords, schemaContainerValidators, schemaReference, schemaDynamicReference, anchor, dynamicAnchor, defsKeyword)
    {
    }

    private static Uri GetBaseUri(Uri? id)
    {
        return id is null || !id.IsAbsoluteUri
            ? DefaultDocumentBaseUri
            : id;
    }

    public void MakeAllIdentifierAndReferenceBeFullUri()
    {
        // here input of 'parentResourceBaseUri' is a arbitrary uri because first schema is schema document which already has real absolute uri
        MakeAllIdentifierAndReferenceBeFullUri(this, DefaultDocumentBaseUri);
    }

    private void MakeAllIdentifierAndReferenceBeFullUri(ISchemaContainerElement containerElement, Uri parentResourceBaseUri)
    {
        Uri currentResourceBaseUri = parentResourceBaseUri;

        if (containerElement is BodyJsonSchema schema)
        {
            schema.ParentResourceBaseUri = parentResourceBaseUri;

            if (schema is JsonSchemaResource schemaResource)
            {
                Debug.Assert(schemaResource.BaseUri is not null);

                currentResourceBaseUri = schemaResource.BaseUri;
                LocalSchemaResourceRegistry.AddSchemaResource(schemaResource);
            }
        }

        foreach (ISchemaContainerElement child in containerElement.EnumerateElements())
        {
            MakeAllIdentifierAndReferenceBeFullUri(child, currentResourceBaseUri);
        }
    }

    public ValidationResult Validate(JsonInstanceElement instance)
    {
        Debug.Assert(GlobalSchemaResourceRegistry is not null);

        var jsonSchemaOptions = new JsonSchemaOptions(GlobalSchemaResourceRegistry);

        Debug.Assert(BaseUri is not null);

        // We need to push current json schema document into path stack here ONLY when this document is 'main' document.
        // For referenced documents, SchemaReferenceKeyword (or SchemaDynamicReferenceKeyword) will take action to push them into path stack.
        jsonSchemaOptions.ValidationPathStack.PushReferencedSchema(this, BaseUri);
        ValidationResult validationResult = Validate(instance, jsonSchemaOptions);
        jsonSchemaOptions.ValidationPathStack.PopReferencedSchema();

        return validationResult;
    }
}