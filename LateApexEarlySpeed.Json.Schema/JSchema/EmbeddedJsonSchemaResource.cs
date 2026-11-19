using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

/// <summary>
/// This represents a standard json schema resource that is embedded inside json schema. It must have an $id.
/// </summary>
internal class EmbeddedJsonSchemaResource : JsonSchemaResource
{
    /// <summary>
    /// Should not contain fragment
    /// </summary>
    private readonly Uri _id;

    public EmbeddedJsonSchemaResource(SchemaKeyword? schemaKeyword, Uri id, IEnumerable<ValidationKeywordBase> keywords, IEnumerable<IAnnotationKeyword>? annotationKeywords, IEnumerable<ISchemaContainerValidationNode>? schemaContainerValidators, IEnumerable<IReferenceKeyword>? referenceKeywords, IPlainNameIdentifierKeyword? plainNameIdentifierKeyword, string? dynamicAnchor, bool recursiveAnchor, IEnumerable<(string name, DefsKeyword keyword)>? defsKeywords, IReadOnlyDictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements, UnevaluatedKeywords unevaluatedKeywords)
        : base(schemaKeyword, keywords, annotationKeywords, schemaContainerValidators, referenceKeywords, plainNameIdentifierKeyword, dynamicAnchor, recursiveAnchor, defsKeywords, potentialSchemaContainerElements, unevaluatedKeywords)
    {
        _id = id;
    }

    public override Uri ParentResourceBaseUri
    {
        set
        {
            BaseUri = new Uri(value, _id);

            string fragment = BaseUri.Fragment;
            if ((!string.IsNullOrEmpty(fragment)) && fragment != "#")
            {
                throw new BadSchemaException("Id of json schema resource should not contain non-empty fragment.");
            }

            base.ParentResourceBaseUri = BaseUri;
        }
    }

    public BodyJsonSchema TransformToBodyJsonSchema()
    {
        var bodyJsonSchema = new BodyJsonSchema(ValidationKeywords, AnnotationKeywords, SchemaContainerValidators, ReferenceKeywords, PlainNameIdentifierKeyword, DynamicAnchor, DefsKeywords, PotentialSchemaContainerElements, new UnevaluatedKeywords { UnevaluatedPropertiesKeyword = UnevaluatedPropertiesKeyword, UnevaluatedItemsKeyword = UnevaluatedItemsKeyword });

        if (Name is not null)
        {
            bodyJsonSchema.Name = Name;
        }

        return bodyJsonSchema;
    }
}