using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BodyJsonSchemaDocument : JsonSchemaResource, IJsonSchemaDocument
{
    public static Uri DefaultDocumentBaseUri { get; } = new("http://lateapexearlyspeed");

    /// <summary>
    /// All schema resources inside current document (including current document itself)
    /// </summary>
    public SchemaResourceRegistry LocalSchemaResourceRegistry { get; } = new(1);

    public SchemaResourceRegistry? GlobalSchemaResourceRegistry { get; set; }

    internal BodyJsonSchemaDocument(IEnumerable<KeywordBase> keywords, IEnumerable<ISchemaContainerValidationNode>? schemaContainerValidators, IEnumerable<IReferenceKeyword>? referenceKeywords, IPlainNameIdentifierKeyword? plainNameIdentifierKeyword, string? dynamicAnchor, bool recursiveAnchor, IReadOnlyDictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements = null, SchemaKeyword? schemaKeyword = null, Uri? id = null, IEnumerable<(string name, DefsKeyword keyword)>? defsKeywords = null)
        : base(schemaKeyword, GetBaseUri(id), keywords, schemaContainerValidators, referenceKeywords, plainNameIdentifierKeyword, dynamicAnchor, recursiveAnchor, defsKeywords, potentialSchemaContainerElements)
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
        // here input of 'parentResourceBaseUri' is an arbitrary uri because first schema is schema document which already has real absolute uri
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

    public ValidationResult DoValidation(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(BaseUri is not null);

        // We need to push current json schema document into path stack here ONLY when this document is 'main' document.
        // For referenced documents, SchemaReferenceKeyword (or SchemaDynamicReferenceKeyword) will take action to push them into path stack.
        options.ValidationPathStack.PushReferencedSchema(this, BaseUri);
        ValidationResult validationResult = Validate(instance, options);
        options.ValidationPathStack.PopReferencedSchema();

        return validationResult;
    }
}