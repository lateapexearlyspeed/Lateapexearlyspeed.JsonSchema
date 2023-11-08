using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp;

internal class JsonSchemaResource : BodyJsonSchema
{
    /// <summary>
    /// Should not contain fragment
    /// </summary>
    private readonly Uri _id;

    public JsonSchemaResource(Uri id, List<KeywordBase> keywords, List<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReference? schemaReference, SchemaDynamicReference? schemaDynamicReference, string? anchor, string? dynamicAnchor, DefsKeyword? defsKeyword) 
        : base(keywords, schemaContainerValidators, schemaReference, schemaDynamicReference, anchor, dynamicAnchor)
    {
        if (!string.IsNullOrEmpty(id.Fragment))
        {
            throw new BadSchemaException("Id of json schema resource should not contain fragment.");
        }

        _id = id;
        DefsKeyword = defsKeyword;
    }

    // Base uri of current schema resource, must be absolute uri
    public Uri? BaseUri { get; private set; }

    public DefsKeyword? DefsKeyword { get; }

    public override Uri ParentResourceBaseUri
    {
        set
        {
            BaseUri = new Uri(value, _id);

            base.ParentResourceBaseUri = BaseUri;
        }
    }

    public JsonSchema? FindSubSchemaByDefs(string defNamePath)
    {
        return DefsKeyword?.GetDefinition(defNamePath);
    }

    public JsonSchema? FindSubSchemaByJsonPointer(string jsonPointerPath)
    {
        var jsonPointer = new JsonPointer(jsonPointerPath);

        ISchemaContainerElement? currentElement = this;
        for (int refTokenIdx = 0; refTokenIdx < jsonPointer.Count; refTokenIdx++)
        {
            string refToken = jsonPointer.GetReferenceToken(refTokenIdx);
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
        return FindSubSchemaBy(this, bodySchema => bodySchema.Anchor == anchorName);
    }

    public BodyJsonSchema? FindSubSchemaByDynamicAnchor(string anchorName)
    {
        return FindSubSchemaBy(this, bodySchema => bodySchema.DynamicAnchor == anchorName);
    }

    private static BodyJsonSchema? FindSubSchemaBy(ISchemaContainerElement schemaContainer, Func<BodyJsonSchema, bool> predicate)
    {
        if (schemaContainer is BodyJsonSchema currentSchema && predicate(currentSchema))
        {
            return currentSchema;
        }

        foreach (ISchemaContainerElement childElement in schemaContainer.EnumerateElements())
        {
            BodyJsonSchema? subSchema = FindSubSchemaBy(childElement, predicate);
            if (subSchema is not null)
            {
                return subSchema;
            }
        }

        return null;
    }

    public override IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        IEnumerable<ISchemaContainerElement> schemaElements = base.EnumerateElements();

        return DefsKeyword is null 
            ? schemaElements 
            : DefsKeyword.GetAllDefinitions().Values.Union(schemaElements);
    }
}