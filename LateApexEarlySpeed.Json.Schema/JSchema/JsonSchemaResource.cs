using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class JsonSchemaResource : BodyJsonSchema
{
    /// <summary>
    /// Should not contain fragment
    /// </summary>
    private readonly Uri _id;

    public JsonSchemaResource(Uri id, List<KeywordBase> keywords, List<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReferenceKeyword? schemaReference, SchemaDynamicReferenceKeyword? schemaDynamicReference, string? anchor, string? dynamicAnchor, DefsKeyword? defsKeyword)
        : base(keywords, schemaContainerValidators, schemaReference, schemaDynamicReference, anchor, dynamicAnchor, defsKeyword)
    {
        _id = id;
    }

    // Base uri of current schema resource, must be absolute uri
    public Uri? BaseUri { get; private set; }

    public override Uri ParentResourceBaseUri
    {
        set
        {
            BaseUri = new Uri(value, _id);

            if (!string.IsNullOrEmpty(BaseUri.Fragment))
            {
                throw new BadSchemaException("Id of json schema resource should not contain fragment.");
            }

            base.ParentResourceBaseUri = BaseUri;
        }
    }

    public JsonSchema? FindSubSchemaByDefs(string defNamePath)
    {
        return DefsKeyword?.GetDefinition(defNamePath);
    }

    public JsonSchema? FindSubSchemaByJsonPointer(string jsonPointerPath)
    {
        JsonPointer? jsonPointer = JsonPointer.Create(jsonPointerPath);
        if (jsonPointer is null)
        {
            return null;
        }

        ISchemaContainerElement? currentElement = this;

        foreach (string refToken in jsonPointer)
        {
            currentElement = currentElement.GetSubElement(refToken);
            if (currentElement is null)
            {
                return null;
            }
        }

        return currentElement.IsSchemaType ? currentElement.GetSchema() : null;
    }

    public BodyJsonSchema? FindSubSchemaByAnchor(string anchorName)
    {
        return FindBodySubSchemaByFilter(this, bodySchema => bodySchema.Anchor == anchorName);
    }

    public BodyJsonSchema? FindSubSchemaByDynamicAnchor(string anchorName)
    {
        return FindBodySubSchemaByFilter(this, bodySchema => bodySchema.DynamicAnchor == anchorName);
    }

    private static BodyJsonSchema? FindBodySubSchemaByFilter(ISchemaContainerElement schemaContainer, Func<BodyJsonSchema, bool> predicate)
    {
        if (schemaContainer is BodyJsonSchema currentSchema && predicate(currentSchema))
        {
            return currentSchema;
        }

        foreach (ISchemaContainerElement childElement in schemaContainer.EnumerateElements())
        {
            // Based on json schema test suite, we should not find subschema from another sub schema resource (with $id), so skip schema resource type.
            if (childElement is JsonSchemaResource)
            {
                continue;
            }

            BodyJsonSchema? subSchema = FindBodySubSchemaByFilter(childElement, predicate);
            if (subSchema is not null)
            {
                return subSchema;
            }
        }

        return null;
    }
}