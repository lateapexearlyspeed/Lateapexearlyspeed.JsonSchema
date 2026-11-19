using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal sealed class BodyJsonSchemaDocument : JsonSchemaResource, IJsonSchemaDocument
{
    public static Uri DefaultDocumentBaseUri { get; } = new("http://lateapexearlyspeed");

    private readonly Uri? _id;

    /// <summary>
    /// All schema resources inside current document (including current document itself and valid retrievalUri-keyed document itself)
    /// </summary>
    public SchemaResourceRegistry LocalSchemaResourceRegistry { get; } = new(1);

    public SchemaResourceRegistry? GlobalSchemaResourceRegistry { get; set; }

    internal BodyJsonSchemaDocument(IEnumerable<ValidationKeywordBase> keywords, IEnumerable<IAnnotationKeyword>? annotationKeywords, IEnumerable<ISchemaContainerValidationNode>? schemaContainerValidators, IEnumerable<IReferenceKeyword>? referenceKeywords, IPlainNameIdentifierKeyword? plainNameIdentifierKeyword, string? dynamicAnchor, bool recursiveAnchor, UnevaluatedKeywords unevaluatedKeywords, IReadOnlyDictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements = null, SchemaKeyword? schemaKeyword = null, Uri? id = null, IEnumerable<(string name, DefsKeyword keyword)>? defsKeywords = null)
        : base(schemaKeyword, keywords, annotationKeywords, schemaContainerValidators, referenceKeywords, plainNameIdentifierKeyword, dynamicAnchor, recursiveAnchor, defsKeywords, potentialSchemaContainerElements, unevaluatedKeywords)
    {
        _id = id;
    }

    public override Uri ParentResourceBaseUri
    {
        set
        {
            BaseUri = _id is null ? value : new Uri(value, _id);

            string fragment = BaseUri.Fragment;
            if ((!string.IsNullOrEmpty(fragment)) && fragment != "#")
            {
                throw new BadSchemaException("Id of json schema resource should not contain non-empty fragment.");
            }

            base.ParentResourceBaseUri = BaseUri;
        }
    }

    public void MakeAllIdentifierAndReferenceBeFullUri(Uri initDocumentBaseUri)
    {
        MakeAllIdentifierAndReferenceBeFullUri(this, initDocumentBaseUri);
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