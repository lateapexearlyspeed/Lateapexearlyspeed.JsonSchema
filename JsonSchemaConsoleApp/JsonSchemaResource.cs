using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp;

internal class JsonSchemaResource : BodyJsonSchema
{
    // Should not contain fragment
    private readonly Uri _id;

    public JsonSchemaResource(Uri id, List<ValidationNode> keywords, ConditionalValidator conditionalValidator, SchemaReference? schemaReference, SchemaDynamicReference? schemaDynamicReference, string? anchor, string? dynamicAnchor, DefsKeyword? defsKeyword) 
        : base(keywords, conditionalValidator, schemaReference, schemaDynamicReference, anchor, dynamicAnchor)
    {
        _id = id;
        if (id.IsAbsoluteUri)
        {
            BaseUri = id;
        }
        DefsKeyword = defsKeyword;
    }

    // Base uri of current schema resource, must be absolute uri
    public Uri? BaseUri { get; private set; }

    public DefsKeyword? DefsKeyword { get; }

    public Uri ParentResourceBaseUri
    {
        set => BaseUri = new Uri(value, _id);
    }

    public JsonSchema? FindSubSchemaByDefs(string defNamePath)
    {
        return DefsKeyword?.GetDefinition(defNamePath);
    }

    public JsonSchema? FindSubSchemaByJsonPointer(string pointer)
    {
        var jsonPointer = new JsonPointer(pointer);

        ISchemaContainerElement? currentElement = this;
        for (int segmentIdx = 0; segmentIdx < jsonPointer.Count; segmentIdx++)
        {
            string segmentName = jsonPointer.GetSegment(segmentIdx);
            currentElement = currentElement.GetSubElement(segmentName);
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