using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BodyJsonSchema : JsonSchema, IJsonSchemaResourceNodesCleanable, IPropertyEvaluator, IItemEvaluator
{
    private class ColdData
    {
        public readonly IAnnotationKeyword[]? AnnotationKeywords;
        public readonly UnevaluatedPropertiesKeyword? UnevaluatedPropertiesKeyword;
        public readonly UnevaluatedItemsKeyword? UnevaluatedItemsKeyword;

        public ColdData(IAnnotationKeyword[]? annotationKeywords, UnevaluatedPropertiesKeyword? unevaluatedPropertiesKeyword, UnevaluatedItemsKeyword? unevaluatedItemsKeyword)
        {
            AnnotationKeywords = annotationKeywords;
            UnevaluatedPropertiesKeyword = unevaluatedPropertiesKeyword;
            UnevaluatedItemsKeyword = unevaluatedItemsKeyword;
        }
    }

    private readonly ISchemaContainerValidationNode[]? _schemaContainerValidators;

    private readonly IReadOnlyList<ValidationKeywordBase> _validationKeywords;
    private readonly IReferenceKeyword[]? _referenceKeywords;
    private readonly ColdData? _coldData;

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
    private readonly (string name, DefsKeyword keyword)[]? _defsKeywords;

    public IReadOnlyList<(string name, DefsKeyword keyword)>? DefsKeywords => _defsKeywords;

    public IReadOnlyList<ISchemaContainerValidationNode>? SchemaContainerValidators => _schemaContainerValidators;

    public IReadOnlyList<ValidationKeywordBase> ValidationKeywords => _validationKeywords;

    public IReadOnlyList<IAnnotationKeyword>? AnnotationKeywords => _coldData?.AnnotationKeywords;

    public IReadOnlyList<IReferenceKeyword>? ReferenceKeywords => _referenceKeywords;

    public UnevaluatedPropertiesKeyword? UnevaluatedPropertiesKeyword => _coldData?.UnevaluatedPropertiesKeyword;

    public UnevaluatedItemsKeyword? UnevaluatedItemsKeyword => _coldData?.UnevaluatedItemsKeyword;

    public IPlainNameIdentifierKeyword? PlainNameIdentifierKeyword { get; }

    public string? DynamicAnchor { get; }

    public BodyJsonSchema(IEnumerable<ValidationKeywordBase> keywords) : this(keywords, null, Enumerable.Empty<ISchemaContainerValidationNode>(), null, null, null, null, null, null, null)
    {

    }

    public BodyJsonSchema(IEnumerable<ValidationKeywordBase> keywords, IEnumerable<IAnnotationKeyword>? annotationKeywords, IEnumerable<ISchemaContainerValidationNode>? schemaContainerValidators, IEnumerable<IReferenceKeyword>? referenceKeywords, IPlainNameIdentifierKeyword? plainNameIdentifierKeyword, string? dynamicAnchor, IEnumerable<(string name, DefsKeyword keyword)>? defsKeywords, IReadOnlyDictionary<string, ISchemaContainerElement>? potentialSchemaContainerElements, UnevaluatedPropertiesKeyword? unevaluatedPropertiesKeyword, UnevaluatedItemsKeyword? unevaluatedItemsKeyword)
    {
        _validationKeywords = MergeKeywords(keywords.ToArray());

        bool hasColdData = false;
        IAnnotationKeyword[]? annotations = null;

        if (annotationKeywords is not null)
        {
            annotations = annotationKeywords.ToArray();
            hasColdData = true;
        }
        else if (unevaluatedPropertiesKeyword is not null || unevaluatedItemsKeyword is not null)
        {
            hasColdData = true;
        }

        if (hasColdData)
        {
            _coldData = new ColdData(annotations, unevaluatedPropertiesKeyword, unevaluatedItemsKeyword);
        }

        if (schemaContainerValidators is not null)
        {
            Debug.Assert(schemaContainerValidators.All(
                validator
                    => validator.GetType() == typeof(ConditionalValidator)
                       || validator.GetType() == typeof(ArrayContainsValidator)));
            _schemaContainerValidators = schemaContainerValidators.ToArray();
        }

        if (referenceKeywords is not null)
        {
            _referenceKeywords = referenceKeywords.ToArray();
        }

        if (defsKeywords is not null)
        {
            _defsKeywords = defsKeywords.ToArray();
        }

        PlainNameIdentifierKeyword = plainNameIdentifierKeyword;
        DynamicAnchor = dynamicAnchor;

        if (potentialSchemaContainerElements is not null)
        {
            _potentialSchemaContainerElements = new Dictionary<string, ISchemaContainerElement>(potentialSchemaContainerElements);
        }
    }

    /// <summary>
    /// Put all duplicated keywords and original 'allOf' keyword(s) into one newly created 'allOf' keyword, so that de-dup them
    /// </summary>
    private static IReadOnlyList<ValidationKeywordBase> MergeKeywords(ValidationKeywordBase[] keywords)
    {
        if (keywords.FindFirstDuplicatedItem(keyword => keyword.Name) is null)
        {
            return keywords;
        }

        var keywordGroups = new Dictionary<string, List<ValidationKeywordBase>>(keywords.Length);
        foreach (ValidationKeywordBase keyword in keywords)
        {
            if (!keywordGroups.TryGetValue(keyword.Name, out List<ValidationKeywordBase>? group))
            {
                // Assume there is no much duplication cases, so initialize 'group' with capacity of 1
                group = new List<ValidationKeywordBase>(1);
                keywordGroups[keyword.Name] = group;
            }

            group.Add(keyword);
        }

        var result = new List<ValidationKeywordBase>(keywords.Length);

        // this includes all duplicated keywords (including 'allOf' keyword)
        var duplicatedKeywords = new List<ValidationKeywordBase>();
        foreach (KeyValuePair<string, List<ValidationKeywordBase>> keywordGroup in keywordGroups)
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
        var validator = new Validator(this, instance, options);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    internal struct Validator
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
        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            IReadOnlyList<ValidationKeywordBase> validationKeywords = _bodyJsonSchema._validationKeywords;

            // ReSharper disable once ForCanBeConvertedToForeach - avoid the cost of enumerator (class System.SZGenericArrayEnumerator<>) allocation of foreach loop for IList<T> from underlying array instance
            for (int i = 0; i < validationKeywords.Count; i++)
            {
                ValidationKeywordBase keyword = validationKeywords[i];

                if (!ReportValidationAndCheckFastReturn(ref this, keyword, ref context))
                {
                    return;
                }
            }

            if (_bodyJsonSchema._schemaContainerValidators is not null)
            {
                foreach (ISchemaContainerValidationNode schemaContainerValidationNode in _bodyJsonSchema._schemaContainerValidators)
                {
                    if (!ReportValidationAndCheckFastReturn(ref this, schemaContainerValidationNode, ref context))
                    {
                        return;
                    }
                }
            }

            if (_bodyJsonSchema._referenceKeywords is not null)
            {
                foreach (IReferenceKeyword referenceKeyword in _bodyJsonSchema._referenceKeywords)
                {
                    if (!ReportValidationAndCheckFastReturn(ref this, referenceKeyword, ref context))
                    {
                        return;
                    }
                }
            }

            if (_bodyJsonSchema._coldData is not null)
            {
                if (_bodyJsonSchema._coldData.UnevaluatedPropertiesKeyword is not null)
                {
                    ReportValidationAndCheckFastReturn(ref this, _bodyJsonSchema._coldData.UnevaluatedPropertiesKeyword, ref context);
                }

                Debug.Assert(_bodyJsonSchema._coldData is not null);
                
                if (_bodyJsonSchema._coldData.UnevaluatedItemsKeyword is not null)
                {
                    ReportValidationAndCheckFastReturn(ref this, _bodyJsonSchema._coldData.UnevaluatedItemsKeyword, ref context);
                }
            }

            static bool ReportValidationAndCheckFastReturn(ref Validator thisValidator, IValidationNode validationNode, ref ValidationCompositionContext context)
            {
                ValidationResult result = validationNode.Validate(thisValidator._instance, thisValidator._options);

                if (!result.IsValid)
                {
                    thisValidator._fastReturnResult = result;
                }

                return context.ReportValidationResult(result, thisValidator._fastReturnResult);
            }
        }

        public void CollectAnnotations(ref ValidationCompositionContext context)
        {
            if (_bodyJsonSchema._coldData?.AnnotationKeywords is not null && context.ShouldAnnotate(IsValid()))
            {
                foreach (IAnnotationKeyword annotationKeyword in _bodyJsonSchema._coldData.AnnotationKeywords)
                {
                    if (annotationKeyword.ShouldAnnotate(_instance))
                    {
                        context.ReportAnnotation(annotationKeyword.Annotate(_instance, _options));
                    }
                }
            }
        }

        public ResultTuple Result => IsValid() ? ResultTuple.Valid() : ResultTuple.Invalid(null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValid() => _fastReturnResult is null;
    }

    public override ISchemaContainerElement? GetSubElement(string name)
    {
        // ReSharper disable once ForCanBeConvertedToForeach - avoid the cost of enumerator (class System.SZGenericArrayEnumerator<>) allocation of foreach loop for IList<T> from underlying array instance
        for (var i = 0; i < _validationKeywords.Count; i++)
        {
            ValidationKeywordBase keyword = _validationKeywords[i];
            if (keyword.Name == name && keyword is ISchemaContainerElement schemaContainerElement)
            {
                return schemaContainerElement;
            }
        }

        if (_schemaContainerValidators is not null)
        {
            foreach (ISchemaContainerValidationNode schemaContainer in _schemaContainerValidators)
            {
                ISchemaContainerElement? schemaContainerElement = schemaContainer.GetSubElement(name);
                if (schemaContainerElement is not null)
                {
                    return schemaContainerElement;
                }
            }
        }

        if (_defsKeywords is not null)
        {
            foreach ((string name, DefsKeyword keyword) defsKeyword in _defsKeywords)
            {
                if (defsKeyword.name == name)
                {
                    return defsKeyword.keyword;
                }
            }
        }

        if (_potentialSchemaContainerElements is not null && _potentialSchemaContainerElements.TryGetValue(name, out ISchemaContainerElement? value))
        {
            return value;
        }

        if (_coldData is null)
        {
            return null;
        }

        if (_coldData.UnevaluatedPropertiesKeyword is not null && _coldData.UnevaluatedPropertiesKeyword.Name == name)
        {
            return _coldData.UnevaluatedPropertiesKeyword;
        }

        if (_coldData.UnevaluatedItemsKeyword is not null && _coldData.UnevaluatedItemsKeyword.Name == name)
        {
            return _coldData.UnevaluatedItemsKeyword;
        }

        return null;
    }

    public override IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        // ReSharper disable once ForCanBeConvertedToForeach - avoid the cost of enumerator (class System.SZGenericArrayEnumerator<>) allocation of foreach loop for IList<T> from underlying array instance
        for (var i = 0; i < _validationKeywords.Count; i++)
        {
            ValidationKeywordBase validationKeyword = _validationKeywords[i];
            if (validationKeyword is ISchemaContainerElement element)
            {
                yield return element;
            }
        }

        IEnumerable<ISchemaContainerElement> schemaContainers = Enumerable.Empty<ISchemaContainerElement>();

        if (_schemaContainerValidators is not null)
        {
            schemaContainers = schemaContainers.Concat(_schemaContainerValidators);
        }

        if (_defsKeywords is not null)
        {
            schemaContainers = schemaContainers.Concat(_defsKeywords.Select(def => def.keyword));
        }

        if (_potentialSchemaContainerElements is not null)
        {
            schemaContainers = schemaContainers.Concat(_potentialSchemaContainerElements.Values);
        }

        if (_coldData is not null)
        {
            if (_coldData.UnevaluatedPropertiesKeyword is not null)
            {
                schemaContainers = schemaContainers.Append(_coldData.UnevaluatedPropertiesKeyword);
            }

            if (_coldData.UnevaluatedItemsKeyword is not null)
            {
                schemaContainers = schemaContainers.Append(_coldData.UnevaluatedItemsKeyword);
            }
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
            if (_referenceKeywords is not null)
            {
                foreach (IReferenceKeyword referenceKeyword in _referenceKeywords)
                {
                    referenceKeyword.ParentResourceBaseUri = value;
                }
            }
        }
    }

    /// <inheritdoc cref="_potentialSchemaContainerElements"/>
    public IReadOnlyDictionary<string, ISchemaContainerElement>? PotentialSchemaContainerElements => _potentialSchemaContainerElements;

    public BodyJsonSchemaDocument TransformToSchemaDocument(Uri id, DefsKeyword defsKeyword)
    {
        return new BodyJsonSchemaDocument(_validationKeywords, _coldData?.AnnotationKeywords, _schemaContainerValidators, _referenceKeywords, PlainNameIdentifierKeyword, DynamicAnchor, false, UnevaluatedPropertiesKeyword, UnevaluatedItemsKeyword, _potentialSchemaContainerElements, null, id, new[] { (DefsKeyword.Keyword, defsKeyword) });
    }

    public BodyJsonSchemaDocument TransformToSchemaDocument(Uri id)
    {
        return new BodyJsonSchemaDocument(_validationKeywords, _coldData?.AnnotationKeywords, _schemaContainerValidators, _referenceKeywords, PlainNameIdentifierKeyword, DynamicAnchor, false, UnevaluatedPropertiesKeyword, UnevaluatedItemsKeyword, _potentialSchemaContainerElements, null, id, _defsKeywords);
    }

    /// <summary>
    /// Remove all ids in <see cref="JsonSchemaResource"/> (replace <see cref="JsonSchemaResource"/> with <see cref="BodyJsonSchema"/>) from <see cref="_potentialSchemaContainerElements"/> trees of current node and all (both valid and invalid keywords) children nodes, recursively.
    /// </summary>
    public void RemoveIdFromAllInvalidKeywordPropertiesRecursively()
    {
        IEnumerable<ISchemaContainerElement> schemaElements = Enumerable.Empty<ISchemaContainerElement>();

        foreach (ValidationKeywordBase keyword in _validationKeywords)
        {
            if (keyword is ISchemaContainerElement schemaContainerElement)
            {
                schemaElements = schemaElements.Append(schemaContainerElement);
            }
        }

        if (_schemaContainerValidators is not null)
        {
            schemaElements = schemaElements.Concat(_schemaContainerValidators);
        }

        if (_defsKeywords is not null)
        {
            schemaElements = schemaElements.Concat(_defsKeywords.Select(def => def.keyword));
        }

        if (_coldData is not null)
        {
            if (_coldData.UnevaluatedPropertiesKeyword is not null)
            {
                schemaElements = schemaElements.Append(_coldData.UnevaluatedPropertiesKeyword);
            }

            if (_coldData.UnevaluatedItemsKeyword is not null)
            {
                schemaElements = schemaElements.Append(_coldData.UnevaluatedItemsKeyword);
            }
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
        foreach (ValidationKeywordBase keyword in _validationKeywords)
        {
            if (keyword is IJsonSchemaResourceNodesCleanable resourceIdCleanable)
            {
                resourceIdCleanable.RemoveIdFromAllChildrenSchemaElements();
            }
        }

        if (_schemaContainerValidators is not null)
        {
            foreach (ISchemaContainerValidationNode schemaContainerValidationNode in _schemaContainerValidators)
            {
                if (schemaContainerValidationNode is IJsonSchemaResourceNodesCleanable resourceIdCleanable)
                {
                    resourceIdCleanable.RemoveIdFromAllChildrenSchemaElements();
                }
            }
        }

        if (_defsKeywords is not null)
        {
            foreach ((string name, DefsKeyword keyword) defsKeyword in _defsKeywords)
            {
                defsKeyword.keyword.RemoveIdFromAllChildrenSchemaElements();
            }
        }

        RemoveIdFromAllChildrenElementsOfPotentialSchemaElementTree();
    }

    public virtual IReadOnlyCollection<string> GetEvaluatedProperties(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var propertyEvaluator = new PropertyEvaluator(this, instance);
        return propertyEvaluator.GetEvaluatedPropertiesWithLocation(options);
    }

    private readonly struct PropertyEvaluator : IPropertyCoreEvaluator
    {
        private readonly BodyJsonSchema _bodyJsonSchema;
        private readonly JsonInstanceElement _instance;

        public PropertyEvaluator(BodyJsonSchema bodyJsonSchema, JsonInstanceElement instance)
        {
            _bodyJsonSchema = bodyJsonSchema;
            _instance = instance;
        }

        public string? Name => _bodyJsonSchema.Name;

        public IReadOnlyCollection<string> GetEvaluatedProperties(JsonSchemaOptions options)
        {
            if (_instance.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<string>();
            }

            if (_bodyJsonSchema._coldData is not null && _bodyJsonSchema._coldData.UnevaluatedPropertiesKeyword is not null) // this means all instance properties are evaluated
            {
                return _instance.EnumerateObject().Select(p => p.Name).ToArray();
            }

            return _bodyJsonSchema.GetEvaluatedInfosInternal<IPropertyEvaluator, string>(_instance, options);
        }
    }

    public virtual IReadOnlyCollection<int> GetEvaluatedItems(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var itemEvaluator = new ItemEvaluator(this, instance);
        return itemEvaluator.GetEvaluatedItemsWithLocation(options);
    }

    private readonly struct ItemEvaluator : IItemCoreEvaluator
    {
        private readonly BodyJsonSchema _bodyJsonSchema;
        private readonly JsonInstanceElement _instance;

        public ItemEvaluator(BodyJsonSchema bodyJsonSchema, JsonInstanceElement instance)
        {
            _bodyJsonSchema = bodyJsonSchema;
            _instance = instance;
        }

        public string? Name => _bodyJsonSchema.Name;

        public IReadOnlyCollection<int> GetEvaluatedItems(JsonSchemaOptions options)
        {
            if (_instance.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<int>();
            }

            if (_bodyJsonSchema._coldData is not null && _bodyJsonSchema._coldData.UnevaluatedItemsKeyword is not null) // this means all array items are evaluated
            {
                return Enumerable.Range(0, _instance.GetArrayLength()).ToArray();
            }

            return _bodyJsonSchema.GetEvaluatedInfosInternal<IItemEvaluator, int>(_instance, options);
        }
    }

    private IReadOnlyCollection<TValue> GetEvaluatedInfosInternal<TEvaluator, TValue>(JsonInstanceElement instance, JsonSchemaOptions options) where TEvaluator : IInfoEvaluator<TValue>
    {
        HashSet<TValue>? result = null;
            
        // ReSharper disable once ForCanBeConvertedToForeach - avoid the cost of enumerator (class System.SZGenericArrayEnumerator<>) allocation of foreach loop for IList<T> from underlying array instance
        for (int i = 0; i < _validationKeywords.Count; i++)
        {
            if (_validationKeywords[i] is TEvaluator propertyEvaluator)
            {
                AddPropertiesOrItems(propertyEvaluator, ref result, instance, options);
            }
        }

        if (_schemaContainerValidators is not null)
        {
            foreach (ISchemaContainerValidationNode schemaContainerValidator in _schemaContainerValidators)
            {
                if (schemaContainerValidator is TEvaluator evaluator)
                {
                    AddPropertiesOrItems(evaluator, ref result, instance, options);
                }
            }
        }

        if (_referenceKeywords is not null)
        {
            foreach (IReferenceKeyword referenceKeyword in _referenceKeywords)
            {
                if (referenceKeyword is TEvaluator evaluator)
                {
                    AddPropertiesOrItems(evaluator, ref result, instance, options);
                }
            }
        }

        return result ?? (IReadOnlyCollection<TValue>)Array.Empty<TValue>();
    }

    private void AddPropertiesOrItems<TEvaluator, TValue>(TEvaluator propertyEvaluator, ref HashSet<TValue>? propertiesOrItems, JsonInstanceElement instance, JsonSchemaOptions options) where TEvaluator : IInfoEvaluator<TValue>
    {
        IReadOnlyCollection<TValue> evaluated = propertyEvaluator.GetEvaluatedInfos(instance, options);
        if (evaluated.Count != 0)
        {
            propertiesOrItems ??= new HashSet<TValue>();
            propertiesOrItems.AddRange(evaluated);
        }
    }

    IReadOnlyCollection<string> IInfoEvaluator<string>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options) => GetEvaluatedProperties(instance, options);

    IReadOnlyCollection<int> IInfoEvaluator<int>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options) => GetEvaluatedItems(instance, options);
}
