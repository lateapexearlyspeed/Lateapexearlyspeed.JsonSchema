using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BodyJsonSchema : JsonSchema, IJsonSchemaResourceNodesCleanable
{
    private readonly ISchemaContainerValidationNode[] _schemaContainerValidators;
    private readonly IReadOnlyList<KeywordBase> _keywords;

    /// <summary>
    /// <see cref="_potentialSchemaContainerElements"/> is used to represent all properties nodes which don't belong to any valid keywords.
    /// It is used to enable reference to any sub nodes as schema. See issue to understand: https://github.com/lateapexearlyspeed/Lateapexearlyspeed.JsonSchema/issues/71
    /// </summary>
    /// <remarks>
    /// Note: Types of nodes inside this tree are: <see cref="BodyJsonSchema"/>, <see cref="JsonSchemaResource"/>, <see cref="BooleanJsonSchema"/> and <see cref="JsonArrayPotentialSchemaContainerElement"/>
    /// </remarks>
    private readonly Dictionary<string, ISchemaContainerElement>? _potentialSchemaContainerElements;

    // {
    //     "$schema": "https://json-schema.org/draft/2020-12/schema",
    //     "$id": "http://example.com/a.json",
    //     "$defs": {
    //         "x": {
    //             "$id": "http://example.com/b/c.json",
    //             "not": {
    //                 "$defs": {
    //                     "y": {
    //                         "$id": "d.json",
    //                         "type": "number"
    //                     }
    //                 }
    //             }
    //         }
    //     },
    //     "allOf": [
    //     {
    //         "$ref": "http://example.com/b/d.json"
    //     }
    //     ]
    // }

    /// <summary>
    /// Pure body json schema is also able to contain '$defs' keyword,
    /// which is ONLY used to enumerate its inner subschema or sub schema resource (with $id)
    /// without functionality of 'find by defs-ref' because pure json schema is not a schema resource (no $id)
    /// </summary>
    public DefsKeyword? DefsKeyword { get; }

    public IReadOnlyList<ISchemaContainerValidationNode> SchemaContainerValidators => _schemaContainerValidators;

    public IReadOnlyList<KeywordBase> Keywords => _keywords;

    public SchemaReferenceKeyword? SchemaReference { get; }

    public SchemaDynamicReferenceKeyword? SchemaDynamicReference { get; }

    public string? Anchor { get; }

    public string? DynamicAnchor { get; }

    public BodyJsonSchema(IEnumerable<KeywordBase> keywords) : this(keywords, Enumerable.Empty<ISchemaContainerValidationNode>(), null, null, null, null, null, null)
    {

    }

    public BodyJsonSchema(IEnumerable<KeywordBase> keywords, IEnumerable<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReferenceKeyword? schemaReference, SchemaDynamicReferenceKeyword? schemaDynamicReference, string? anchor, string? dynamicAnchor, DefsKeyword? defsKeyword, IReadOnlyDictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements)
    {
        _keywords = MergeKeywords(keywords.ToArray());

        Debug.Assert(schemaContainerValidators.All(
                validator
                    => validator.GetType() == typeof(ConditionalValidator)
                    || validator.GetType() == typeof(ArrayContainsValidator)));
        _schemaContainerValidators = schemaContainerValidators.ToArray();

        SchemaReference = schemaReference;
        SchemaDynamicReference = schemaDynamicReference;
        
        DefsKeyword = defsKeyword;

        Anchor = anchor;
        DynamicAnchor = dynamicAnchor;

        if (potentialSchemaContainerElements is not null)
        {
            _potentialSchemaContainerElements = new Dictionary<string, ISchemaContainerElement>(potentialSchemaContainerElements);
        }
    }

    /// <summary>
    /// Put all duplicated keywords and original 'allOf' keyword(s) into one newly created 'allOf' keyword, so that de-dup them
    /// </summary>
    private static IReadOnlyList<KeywordBase> MergeKeywords(KeywordBase[] keywords)
    {
        if (keywords.FindFirstDuplicatedItem(keyword => keyword.Name) is null)
        {
            return keywords;
        }

        var keywordGroups = new Dictionary<string, List<KeywordBase>>(keywords.Length);
        foreach (KeywordBase keyword in keywords)
        {
            if (!keywordGroups.TryGetValue(keyword.Name, out List<KeywordBase>? group))
            {
                // Assume there is no much duplication cases, so initialize 'group' with capacity of 1
                group = new List<KeywordBase>(1);
                keywordGroups[keyword.Name] = group;
            }

            group.Add(keyword);
        }

        var result = new List<KeywordBase>(keywords.Length);

        // this includes all duplicated keywords (including 'allOf' keyword)
        var duplicatedKeywords = new List<KeywordBase>();
        foreach (KeyValuePair<string, List<KeywordBase>> keywordGroup in keywordGroups)
        {
            if (keywordGroup.Key == AllOfKeyword.Keyword) // found 'allOf' keyword(s)
            {
                duplicatedKeywords.AddRange(keywordGroup.Value);
            }
            else if (keywordGroup.Value.Count > 1) // found duplicated keyword group
            {
                duplicatedKeywords.AddRange(keywordGroup.Value);
            }
            else
            {
                result.Add(keywordGroup.Value[0]);
            }
        }

        Debug.Assert(duplicatedKeywords.Count != 0);

        // Combine all duplicated keywords into ONE new 'allOf' keyword
        var allOfKeyword = new AllOfKeyword(duplicatedKeywords.Select(k => new BodyJsonSchema(new[] { k })) );
        result.Add(allOfKeyword);

        return result;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return ValidationResultsComposer.Compose(new Validator(this, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
    {
        private readonly BodyJsonSchema _bodyJsonSchema;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(BodyJsonSchema bodyJsonSchema, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _bodyJsonSchema = bodyJsonSchema;
            _instance = instance;
            _options = options;
        }

        /// <remarks>
        /// Previous implementation of this method was by using linq Concat() and linq Append() to chain all validation nodes then iterate each item to call its Validation() method.
        /// Although it was more readable, the profiler showed linq Concat() taking high cpu time percentage during validation of <see cref="BodyJsonSchema"/> which is in frequent call path.
        /// That is the reason to change to implement it by validating validation-node-collections one by one.
        /// </remarks>
        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (KeywordBase keyword in _bodyJsonSchema._keywords)
            {
                yield return ValidateAndSetFastReturnResult(keyword);
            }

            foreach (ISchemaContainerValidationNode schemaContainerValidationNode in _bodyJsonSchema._schemaContainerValidators)
            {
                yield return ValidateAndSetFastReturnResult(schemaContainerValidationNode);
            }

            if (_bodyJsonSchema.SchemaReference is not null)
            {
                yield return ValidateAndSetFastReturnResult(_bodyJsonSchema.SchemaReference);
            }

            if (_bodyJsonSchema.SchemaDynamicReference is not null)
            {
                yield return ValidateAndSetFastReturnResult(_bodyJsonSchema.SchemaDynamicReference);
            }

            ValidationResult ValidateAndSetFastReturnResult(IValidationNode validationNode)
            {
                ValidationResult result = validationNode.Validate(_instance, _options);
                if (!result.IsValid)
                {
                    _fastReturnResult = result;
                }

                return result;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            return (validationResult = _fastReturnResult) is not null;
        }

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }

    public override ISchemaContainerElement? GetSubElement(string name)
    {
        foreach (KeywordBase keyword in _keywords)
        {
            if (keyword.Name == name && keyword is ISchemaContainerElement schemaContainerElement)
            {
                return schemaContainerElement;
            }
        }

        foreach (ISchemaContainerValidationNode schemaContainer in _schemaContainerValidators)
        {
            ISchemaContainerElement? schemaContainerElement = schemaContainer.GetSubElement(name);
            if (schemaContainerElement is not null)
            {
                return schemaContainerElement;
            }
        }

        if (name == DefsKeyword.Keyword)
        {
            return DefsKeyword;
        }

        return _potentialSchemaContainerElements?.GetValueOrDefault(name);
    }

    public override IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        foreach (KeywordBase validationKeyword in _keywords)
        {
            if (validationKeyword is ISchemaContainerElement element)
            {
                yield return element;
            }
        }

        IEnumerable<ISchemaContainerElement> schemaContainers = _schemaContainerValidators;
        if (DefsKeyword is not null)
        {
            schemaContainers = schemaContainers.Append(DefsKeyword);
        }

        if (_potentialSchemaContainerElements is not null)
        {
            schemaContainers = schemaContainers.Concat(_potentialSchemaContainerElements.Values);
        }

        foreach (ISchemaContainerElement containerElement in schemaContainers)
        {
            yield return containerElement;
        }
    }

    public virtual Uri ParentResourceBaseUri
    {
        set
        {
            if (SchemaReference is not null)
            {
                SchemaReference.ParentResourceBaseUri = value;
            }

            if (SchemaDynamicReference is not null)
            {
                SchemaDynamicReference.ParentResourceBaseUri = value;
            }
        }
    }

    /// <inheritdoc cref="_potentialSchemaContainerElements"/>
    public IReadOnlyDictionary<string, ISchemaContainerElement>? PotentialSchemaContainerElements => _potentialSchemaContainerElements;

    public BodyJsonSchemaDocument TransformToSchemaDocument(Uri id, DefsKeyword defsKeyword)
    {
        return new BodyJsonSchemaDocument(_keywords, _schemaContainerValidators, SchemaReference, SchemaDynamicReference, Anchor, DynamicAnchor, _potentialSchemaContainerElements, id, defsKeyword);
    }

    public BodyJsonSchemaDocument TransformToSchemaDocument(Uri id)
    {
        return new BodyJsonSchemaDocument(_keywords, _schemaContainerValidators, SchemaReference, SchemaDynamicReference, Anchor, DynamicAnchor, _potentialSchemaContainerElements, id, DefsKeyword);
    }

    /// <summary>
    /// Remove all ids in <see cref="JsonSchemaResource"/> (replace <see cref="JsonSchemaResource"/> with <see cref="BodyJsonSchema"/>) from <see cref="_potentialSchemaContainerElements"/> trees of current node and all (both valid and invalid keywords) children nodes, recursively.
    /// </summary>
    public void RemoveIdFromAllInvalidKeywordPropertiesRecursively()
    {
        IEnumerable<ISchemaContainerElement> schemaElements = Enumerable.Empty<ISchemaContainerElement>();

        foreach (KeywordBase keyword in _keywords)
        {
            if (keyword is ISchemaContainerElement schemaContainerElement)
            {
                schemaElements = schemaElements.Append(schemaContainerElement);
            }
        }

        schemaElements = schemaElements.Concat(_schemaContainerValidators);
        if (DefsKeyword is not null)
        {
            schemaElements = schemaElements.Append(DefsKeyword);
        }

        foreach (ISchemaContainerElement schemaElement in schemaElements)
        {
            RemoveIdFromAllInvalidKeywordPropertiesRecursivelyInternal(schemaElement);
        }

        RemoveIdFromAllChildrenElementsOfPotentialSchemaElementTree();
    }

    private static void RemoveIdFromAllInvalidKeywordPropertiesRecursivelyInternal(ISchemaContainerElement schemaElement)
    {
        if (schemaElement.IsSchemaType)
        {
            JsonSchema jsonSchema = schemaElement.GetSchema();

            if (jsonSchema is BodyJsonSchema bodyJsonSchema)
            {
                bodyJsonSchema.RemoveIdFromAllInvalidKeywordPropertiesRecursively();
            }

            return;
        }

        foreach (ISchemaContainerElement schemaContainerElement in schemaElement.EnumerateElements())
        {
            RemoveIdFromAllInvalidKeywordPropertiesRecursivelyInternal(schemaContainerElement);
        }
    }

    /// <summary>
    /// Remove all ids in <see cref="JsonSchemaResource"/> (replace <see cref="JsonSchemaResource"/> with <see cref="BodyJsonSchema"/>) from <see cref="_potentialSchemaContainerElements"/> trees of current node recursively, but not including valid keywords trees on current node.
    /// </summary>
    private void RemoveIdFromAllChildrenElementsOfPotentialSchemaElementTree()
    {
        if (_potentialSchemaContainerElements is null)
        {
            return;
        }

        foreach ((string propertyName, ISchemaContainerElement schemaElement) in _potentialSchemaContainerElements)
        {
            if (schemaElement is IJsonSchemaResourceNodesCleanable jsonSchemaResourceNodesCleanable)
            {
                jsonSchemaResourceNodesCleanable.RemoveIdFromAllChildrenSchemaElements();

                if (schemaElement is JsonSchemaResource jsonSchemaResource)
                {
                    BodyJsonSchema newSchema = jsonSchemaResource.TransformToBodyJsonSchema();
                    _potentialSchemaContainerElements[propertyName] = newSchema;
                }
            }
        }
    }

    /// <summary>
    /// Remove all id for all children nodes in <paramref name="rootNode"/> tree.
    /// Also remove id of current <paramref name="rootNode"/> itself (which means it will be transformed to <see cref="BodyJsonSchema"/> if it currently is <see cref="JsonSchemaResource"/>)
    /// </summary>
    /// <param name="rootNode"></param>
    /// <param name="saveNewSchema">If <paramref name="rootNode"/> is <see cref="JsonSchemaResource"/>, transform happens and <paramref name="saveNewSchema"/> is called.</param>
    internal static void RemoveIdForBodyJsonSchemaTree(BodyJsonSchema rootNode, Action<BodyJsonSchema> saveNewSchema)
    {
        BodyJsonSchema bodyJsonSchema = rootNode;

        if (rootNode is JsonSchemaResource jsonSchemaResource)
        {
            bodyJsonSchema = jsonSchemaResource.TransformToBodyJsonSchema();

            saveNewSchema(bodyJsonSchema);
        }

        bodyJsonSchema.RemoveIdFromAllChildrenSchemaElements();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        foreach (KeywordBase keyword in _keywords)
        {
            if (keyword is IJsonSchemaResourceNodesCleanable resourceIdCleanable)
            {
                resourceIdCleanable.RemoveIdFromAllChildrenSchemaElements();
            }
        }

        foreach (ISchemaContainerValidationNode schemaContainerValidationNode in _schemaContainerValidators)
        {
            if (schemaContainerValidationNode is IJsonSchemaResourceNodesCleanable resourceIdCleanable)
            {
                resourceIdCleanable.RemoveIdFromAllChildrenSchemaElements();
            }
        }

        if (DefsKeyword is not null)
        {
            DefsKeyword.RemoveIdFromAllChildrenSchemaElements();
        }

        RemoveIdFromAllChildrenElementsOfPotentialSchemaElementTree();
    }
}