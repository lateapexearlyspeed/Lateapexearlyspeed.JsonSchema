using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class ConditionalValidator : ISchemaContainerValidationNode, IJsonSchemaResourceNodesCleanable, IPropertyEvaluator, IItemEvaluator
{
    public const string IfKeywordName = "if";
    public const string ThenKeywordName = "then";
    public const string ElseKeywordName = "else";

    private JsonSchema? _predictEvaluator;
    private JsonSchema _positiveValidator;
    private JsonSchema _negativeValidator;

    public JsonSchema? PredictEvaluator => _predictEvaluator;
    public JsonSchema PositiveValidator => _positiveValidator;
    public JsonSchema NegativeValidator => _negativeValidator;

    public ConditionalValidator(JsonSchema? predictEvaluator, JsonSchema? positiveValidator, JsonSchema? negativeValidator)
    {
        _predictEvaluator = predictEvaluator;
        if (_predictEvaluator is not null)
        {
            _predictEvaluator.Name = IfKeywordName;
        }

        _positiveValidator = positiveValidator ?? BooleanJsonSchema.True;
        _positiveValidator.Name = ThenKeywordName;

        _negativeValidator = negativeValidator ?? BooleanJsonSchema.True;
        _negativeValidator.Name = ElseKeywordName;
    }

    public ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (PredictEvaluator is null)
        {
            return ValidationResult.ValidResult;
        }

        bool isInUnevaluatedScope = options.InUnevaluatedPropertiesScope || options.InUnevaluatedItemsScope;

        if (options.OutputFormat == OutputFormat.FailFast)
        {
            bool isValid = PredictEvaluator.Validate(instance, options).IsValid;

            if (isInUnevaluatedScope)
            {
                options.SetIfSubSchemaStatus(this, instance.Location, isValid);
            }

            return isValid ? PositiveValidator.Validate(instance, options) : NegativeValidator.Validate(instance, options);
        }

        var errorsBuilder = new ImmutableValidationErrorCollection.Builder();
        var annotationBuilder = new ImmutableDoubleEndedLinkedList<Annotation>.Builder();

        ValidationResult predictResult = PredictEvaluator.Validate(instance, options);
        errorsBuilder.AddChildCollection(predictResult.ValidationErrorsList);
        annotationBuilder.AddRange(predictResult.AnnotationList);

        if (isInUnevaluatedScope)
        {
            options.SetIfSubSchemaStatus(this, instance.Location, predictResult.IsValid);
        }

        if (predictResult.IsValid)
        {
            ValidationResult positiveResult = PositiveValidator.Validate(instance, options);
            errorsBuilder.AddChildCollection(positiveResult.ValidationErrorsList);
            annotationBuilder.AddRange(positiveResult.AnnotationList);

            return new ValidationResult(positiveResult.IsValid, errorsBuilder.ToImmutable(), annotationBuilder.ToImmutable());
        }

        ValidationResult negativeResult = NegativeValidator.Validate(instance, options);
        errorsBuilder.AddChildCollection(negativeResult.ValidationErrorsList);
        annotationBuilder.AddRange(negativeResult.AnnotationList);

        return new ValidationResult(negativeResult.IsValid, errorsBuilder.ToImmutable(), annotationBuilder.ToImmutable());
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        switch (name)
        {
            case IfKeywordName:
                return PredictEvaluator;
            case ThenKeywordName:
                return PositiveValidator;
            case ElseKeywordName:
                return NegativeValidator;
            default:
                return null;
        }
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        if (PredictEvaluator is not null)
        {
            yield return PredictEvaluator;
        }
        
        yield return PositiveValidator;
        yield return NegativeValidator;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        if (_predictEvaluator is BodyJsonSchema predictEvaluatorSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(predictEvaluatorSchema, newSchema => _predictEvaluator = newSchema);
        }

        if (_positiveValidator is BodyJsonSchema positiveValidatorSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(positiveValidatorSchema, newSchema => _positiveValidator = newSchema);
        }

        if (_negativeValidator is BodyJsonSchema negativeValidatorSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(negativeValidatorSchema, newSchema => _negativeValidator = newSchema);
        }
    }

    IReadOnlyCollection<string> IInfoEvaluator<string>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return GetEvaluatedInfo<IPropertyEvaluator, string>(instance, options);
    }

    IReadOnlyCollection<int> IInfoEvaluator<int>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return GetEvaluatedInfo<IItemEvaluator, int>(instance, options);
    }

    private IReadOnlyCollection<TValue> GetEvaluatedInfo<TEvaluator, TValue>(JsonInstanceElement instance, JsonSchemaOptions options) where TEvaluator : IInfoEvaluator<TValue>
    {
        if (PredictEvaluator is null)
        {
            return Array.Empty<TValue>();
        }

        HashSet<TValue>? evaluatedProperties = null;

        bool ifStatus = options.GetIfSubSchemaStatus(this, instance.Location);

        if (ifStatus)
        {
            if (PredictEvaluator is TEvaluator predictEvaluator)
            {
                IReadOnlyCollection<TValue> predictProperties = predictEvaluator.GetEvaluatedInfos(instance, options);
                if (predictProperties.Count != 0)
                {
                    evaluatedProperties ??= new HashSet<TValue>();
                    evaluatedProperties.UnionWith(predictProperties);
                }
            }

            if (PositiveValidator is TEvaluator positiveEvaluator)
            {
                IReadOnlyCollection<TValue> positiveProperties = positiveEvaluator.GetEvaluatedInfos(instance, options);
                if (positiveProperties.Count != 0)
                {
                    evaluatedProperties ??= new HashSet<TValue>();
                    evaluatedProperties.UnionWith(positiveProperties);
                }
            }
        }
        else
        {
            if (NegativeValidator is TEvaluator negativeEvaluator)
            {
                IReadOnlyCollection<TValue> negativeProperties = negativeEvaluator.GetEvaluatedInfos(instance, options);
                if (negativeProperties.Count != 0)
                {
                    evaluatedProperties ??= new HashSet<TValue>();
                    evaluatedProperties.UnionWith(negativeProperties);
                }
            }
        }
        
        return evaluatedProperties ?? (IReadOnlyCollection<TValue>)Array.Empty<TValue>();
    }

}