using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

/// <summary>
/// This represents base class of <see cref="BodyJsonSchemaDocument"/> and <see cref="EmbeddedJsonSchemaResource"/>. It has common features of json schema resource.
/// </summary>
/// <remarks>It does not have to have an $id because <see cref="BodyJsonSchemaDocument"/> does not have to have it</remarks>
internal abstract class JsonSchemaResource : BodyJsonSchema, IInfoEvaluator<string>, IInfoEvaluator<int>
{
    protected JsonSchemaResource(SchemaKeyword? schemaKeyword, IEnumerable<ValidationKeywordBase> keywords, IEnumerable<IAnnotationKeyword>? annotationKeywords, IEnumerable<ISchemaContainerValidationNode>? schemaContainerValidators, IEnumerable<IReferenceKeyword>? referenceKeywords, IPlainNameIdentifierKeyword? plainNameIdentifierKeyword, string? dynamicAnchor, bool recursiveAnchor, IEnumerable<(string name, DefsKeyword keyword)>? defsKeywords, IReadOnlyDictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements, UnevaluatedKeywords unevaluatedKeywords)
        : base(keywords, annotationKeywords, schemaContainerValidators, referenceKeywords, plainNameIdentifierKeyword, dynamicAnchor, defsKeywords, potentialSchemaContainerElements, unevaluatedKeywords)
    {
        SchemaKeyword = schemaKeyword;
        RecursiveAnchor = recursiveAnchor;
    }

    public override SchemaKeyword? SchemaKeyword { get; }
    public bool RecursiveAnchor { get; }

    // Base uri of current schema resource, must be absolute uri
    public Uri? BaseUri { get; protected set; }
    
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
            if (childElement is EmbeddedJsonSchemaResource)
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

    public bool RecursiveAnchorEnabled => RecursiveAnchor;

    public override ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        options.ValidationPathStack.PushSchemaResource(this);

        ValidationResult validationResult = base.Validate(instance, options);

        options.ValidationPathStack.PopSchemaResource();

        return validationResult;
    }

    IReadOnlyCollection<string> IInfoEvaluator<string>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        options.ValidationPathStack.PushSchemaResource(this);

        IReadOnlyCollection<string> result = GetEvaluatedPropertiesInternal(instance, options);

        options.ValidationPathStack.PopSchemaResource();

        return result;
    }

    IReadOnlyCollection<int> IInfoEvaluator<int>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        options.ValidationPathStack.PushSchemaResource(this);

        IReadOnlyCollection<int> result = GetEvaluatedItemsInternal(instance, options);

        options.ValidationPathStack.PopSchemaResource();

        return result;
    }
}