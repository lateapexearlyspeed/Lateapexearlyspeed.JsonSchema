using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class JsonSchemaResource : BodyJsonSchema
{
    /// <summary>
    /// Should not contain fragment
    /// </summary>
    private readonly Uri _id;

    public JsonSchemaResource(SchemaKeyword? schemaKeyword, Uri id, IEnumerable<KeywordBase> keywords, IEnumerable<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReferenceKeyword? schemaReference, SchemaDynamicReferenceKeyword? schemaDynamicReference, IPlainNameIdentifierKeyword? plainNameIdentifierKeyword, string? dynamicAnchor, IEnumerable<(string name, DefsKeyword keyword)>? defsKeywords, IReadOnlyDictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements)
        : base(keywords, schemaContainerValidators, schemaReference, schemaDynamicReference, plainNameIdentifierKeyword, dynamicAnchor, defsKeywords, potentialSchemaContainerElements)
    {
        SchemaKeyword = schemaKeyword;
        _id = id;
    }

    public SchemaKeyword? SchemaKeyword { get; }

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

    public JsonSchema? FindSubSchemaByJsonPointer(string jsonPointerPath)
    {
        LinkedListBasedImmutableJsonPointer? jsonPointer = LinkedListBasedImmutableJsonPointer.Create(jsonPointerPath);
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

    public BodyJsonSchema? FindSubSchemaByPlainNameIdentifier(string plainNameIdentifier)
    {
        return FindBodySubSchemaByFilter(this, bodySchema => bodySchema.PlainNameIdentifierKeyword is not null && bodySchema.PlainNameIdentifierKeyword.Identifier == plainNameIdentifier);
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

    public override ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        options.ValidationPathStack.PushSchemaResource(this);

        ValidationResult validationResult = base.Validate(instance, options);

        options.ValidationPathStack.PopSchemaResource();

        return validationResult;
    }

    public BodyJsonSchema TransformToBodyJsonSchema()
    {
        var bodyJsonSchema = new BodyJsonSchema(Keywords, SchemaContainerValidators, SchemaReference, SchemaDynamicReference, PlainNameIdentifierKeyword, DynamicAnchor, DefsKeywords, PotentialSchemaContainerElements);

        if (Name is not null)
        {
            bodyJsonSchema.Name = Name;
        }

        return bodyJsonSchema;
    }
}