using System.Diagnostics;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class ArrayContainsValidator : ISchemaContainerValidationNode, IJsonSchemaResourceNodesCleanable, IItemEvaluator
{
    public const string ContainsKeywordName = "contains";
    public const string MaxContainsKeywordName = "maxContains";
    public const string MinContainsKeywordName = "minContains";
    
    private JsonSchema _containsSchema;

    public JsonSchema ContainsSchema => _containsSchema;

    public uint? MinContains { get; }
    public uint? MaxContains { get; }

    public ArrayContainsValidator(JsonSchema containsSchema, uint? minContains, uint? maxContains)
    {
        containsSchema.Name = ContainsKeywordName;
        _containsSchema = containsSchema;

        MinContains = minContains;
        MaxContains = maxContains;
    }

    public ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        // 'contains' evaluated items info is only consumed by 'unevaluatedItems', so only need to record when inside 'unevaluatedItems' scope.
        // Note this must be read before resetting scope below.
        bool shouldRecordEvaluatedItems = options.InUnevaluatedItemsScope;

        // 'contains' sub schema applies to array items, which start new unevaluated scopes.
        bool origPropState = options.EnterUnevaluatedPropertiesScope(false);
        bool origItemState = options.EnterUnevaluatedItemsScope(false);

        ValidationResult result = ValidateInternal(instance, options, shouldRecordEvaluatedItems);

        options.ExitUnevaluatedItemsScope(origItemState);
        options.ExitUnevaluatedPropertiesScope(origPropState);

        return result;
    }

    private ValidationResult ValidateInternal(JsonInstanceElement instance, JsonSchemaOptions options, bool shouldRecordEvaluatedItems)
    {
        if (!MinContains.HasValue && !MaxContains.HasValue)
        {
            return ValidateWithoutMinContainsAndMaxContains(instance, options, shouldRecordEvaluatedItems);
        }

        if (MaxContains.HasValue)
        {
            return ValidateWithMaxContains(instance, options, shouldRecordEvaluatedItems);
        }

        Debug.Assert(!MaxContains.HasValue);
        Debug.Assert(MinContains.HasValue);

        return ValidateWithoutMaxContains(instance, options, shouldRecordEvaluatedItems);
    }

    private ValidationResult ValidateWithoutMaxContains(JsonInstanceElement instance, JsonSchemaOptions options, bool shouldRecordEvaluatedItems)
    {
        Debug.Assert(MinContains.HasValue);

        // When not recording evaluated items, 'minContains == 0' makes this keyword trivially pass without validating any item.
        // When recording, still need to iterate items to record matched evaluated items (result is always valid since minContains is 0).
        if (MinContains == 0 && !shouldRecordEvaluatedItems)
        {
            return ValidationResult.ValidResult;
        }

        var validator = new WithoutMaxContainsValidator(this, instance, options, shouldRecordEvaluatedItems);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    private struct WithoutMaxContainsValidator : IValidator
    {
        private readonly ArrayContainsValidator _arrayContainsValidator;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        private readonly bool _shouldRecordEvaluatedItems;
        private int _validatedItemCount;

        public WithoutMaxContainsValidator(ArrayContainsValidator arrayContainsValidator, JsonInstanceElement instance, JsonSchemaOptions options, bool shouldRecordEvaluatedItems)
        {
            _arrayContainsValidator = arrayContainsValidator;
            _instance = instance;
            _options = options;
            _shouldRecordEvaluatedItems = shouldRecordEvaluatedItems;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            Debug.Assert(_arrayContainsValidator.ContainsSchema is not null);

            if (_shouldRecordEvaluatedItems)
            {
                byte[] evaluatedItemsStatus = BitmapHelper.CreateBitmap(_instance.GetArrayLength());

                int i = 0;
                foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
                {
                    ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                    if (validationResult.IsValid)
                    {
                        _validatedItemCount++;
                        BitmapHelper.SetBit(evaluatedItemsStatus, i);
                    }

                    bool reportValidationResult = context.ReportValidationResult(validationResult, null);
                    Debug.Assert(reportValidationResult);

                    i++;
                }

                _options.SetSubSchemaStatusBitmap(_arrayContainsValidator, _instance.Location, evaluatedItemsStatus);
            }
            else
            {
                foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
                {
                    ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                    if (validationResult.IsValid)
                    {
                        _validatedItemCount++;
                    }

                    ValidationResult? fastResult = _validatedItemCount >= _arrayContainsValidator.MinContains
                        ? ValidationResult.ValidResult
                        : null;

                    if (!context.ReportValidationResult(validationResult, fastResult))
                    {
                        break;
                    }
                }
            }
        }

        public ResultTuple Result
        {
            get
            {
                Debug.Assert(_arrayContainsValidator.MinContains.HasValue);

                if (_validatedItemCount >= _arrayContainsValidator.MinContains)
                {
                    return ResultTuple.Valid();
                }

                ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, _options.GenerateErrorMessages ? GetFailedMinContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MinContains.Value) : string.Empty, MinContainsKeywordName, _options.ValidationPathStack, _instance.Location);
                return ResultTuple.Invalid(error);
            }
        }
    }

    private ValidationResult ValidateWithMaxContains(JsonInstanceElement instance, JsonSchemaOptions options, bool shouldRecordEvaluatedItems)
    {
        var validator = new WithMaxContainsValidator(this, instance, options, shouldRecordEvaluatedItems);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    private struct WithMaxContainsValidator : IValidator
    {
        private readonly ArrayContainsValidator _arrayContainsValidator;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        private readonly bool _shouldRecordEvaluatedItems;
        private int _validatedItemCount;

        public WithMaxContainsValidator(ArrayContainsValidator arrayContainsValidator, JsonInstanceElement instance, JsonSchemaOptions options, bool shouldRecordEvaluatedItems)
        {
            _arrayContainsValidator = arrayContainsValidator;
            _instance = instance;
            _options = options;
            _shouldRecordEvaluatedItems = shouldRecordEvaluatedItems;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            Debug.Assert(_arrayContainsValidator.ContainsSchema is not null);

            if (_shouldRecordEvaluatedItems)
            {
                byte[] evaluatedItemsStatus = BitmapHelper.CreateBitmap(_instance.GetArrayLength());

                int i = 0;
                foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
                {
                    ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                    if (validationResult.IsValid)
                    {
                        _validatedItemCount++;
                        BitmapHelper.SetBit(evaluatedItemsStatus, i);
                    }

                    bool reportValidationResult = context.ReportValidationResult(validationResult, null);
                    Debug.Assert(reportValidationResult);

                    i++;
                }

                _options.SetSubSchemaStatusBitmap(_arrayContainsValidator, _instance.Location, evaluatedItemsStatus);
            }
            else
            {
                foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
                {
                    ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                    if (validationResult.IsValid)
                    {
                        _validatedItemCount++;
                    }

                    ValidationResult? fastResult = null;
                    if (_validatedItemCount > _arrayContainsValidator.MaxContains)
                    {
                        ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, _options.GenerateErrorMessages ? GetFailedMaxContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MaxContains.Value) : string.Empty, MaxContainsKeywordName, _options.ValidationPathStack, _instance.Location);
                        fastResult = ValidationResult.SingleErrorFailedResult(error);
                    }

                    if (!context.ReportValidationResult(validationResult, fastResult))
                    {
                        break;
                    }
                }
            }
        }

        public ResultTuple Result
        {
            get
            {
                if (_validatedItemCount > _arrayContainsValidator.MaxContains)
                {
                    ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, _options.GenerateErrorMessages ? GetFailedMaxContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MaxContains.Value) : string.Empty, MaxContainsKeywordName, _options.ValidationPathStack, _instance.Location);
                    return ResultTuple.Invalid(error);
                }

                if (_arrayContainsValidator.MinContains.HasValue)
                {
                    if (_validatedItemCount < _arrayContainsValidator.MinContains)
                    {
                        ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, _options.GenerateErrorMessages ? GetFailedMinContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MinContains.Value) : string.Empty, MinContainsKeywordName, _options.ValidationPathStack, _instance.Location);

                        return ResultTuple.Invalid(error);
                    }
                }
                else
                {
                    if (_validatedItemCount == 0)
                    {
                        ValidationError error = CreateValidationErrorWithLocation(ResultCode.NotFoundAnyValidatedArrayItem, _options.GenerateErrorMessages ? GetFailedContainsErrorMessage(_instance.ToString()) : string.Empty, ContainsKeywordName, _options.ValidationPathStack, _instance.Location);

                        return ResultTuple.Invalid(error);
                    }
                }

                return ResultTuple.Valid();
            }
        }
    }

    internal static string GetFailedMaxContainsErrorMessage(string instanceJson, uint maxContains)
        => $"Validated array item count exceeds maximum '{maxContains}', array instance: {instanceJson}";

    internal static string GetFailedMinContainsErrorMessage(string instanceJson, uint minContains)
        => $"Validated array item count is less than minimum '{minContains}', array instance: {instanceJson}";

    internal static string GetFailedContainsErrorMessage(string instanceJson)
        => $"No validated array items found, array instance: {instanceJson}";

    private ValidationResult ValidateWithoutMinContainsAndMaxContains(JsonInstanceElement instance, JsonSchemaOptions options, bool shouldRecordEvaluatedItems)
    {
        var validator = new WithoutMinContainsAndMaxContainsValidator(this, instance, options, shouldRecordEvaluatedItems);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    private struct WithoutMinContainsAndMaxContainsValidator : IValidator
    {
        private readonly ArrayContainsValidator _arrayContainsValidator;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        private readonly bool _shouldRecordEvaluatedItems;
        private ValidationResult? _fastReturnResult;

        public WithoutMinContainsAndMaxContainsValidator(ArrayContainsValidator arrayContainsValidator, JsonInstanceElement instance, JsonSchemaOptions options, bool shouldRecordEvaluatedItems)
        {
            _arrayContainsValidator = arrayContainsValidator;
            _instance = instance;
            _options = options;
            _shouldRecordEvaluatedItems = shouldRecordEvaluatedItems;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            Debug.Assert(_arrayContainsValidator.ContainsSchema is not null);

            if (_shouldRecordEvaluatedItems)
            {
                byte[] evaluatedItemsStatus = BitmapHelper.CreateBitmap(_instance.GetArrayLength());

                int i = 0;
                foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
                {
                    ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                    if (validationResult.IsValid)
                    {
                        _fastReturnResult = ValidationResult.ValidResult;
                        BitmapHelper.SetBit(evaluatedItemsStatus, i);
                    }

                    bool reportValidationResult = context.ReportValidationResult(validationResult, null);
                    Debug.Assert(reportValidationResult);

                    i++;
                }

                _options.SetSubSchemaStatusBitmap(_arrayContainsValidator, _instance.Location, evaluatedItemsStatus);
            }
            else
            {
                foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
                {
                    ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                    if (validationResult.IsValid)
                    {
                        _fastReturnResult = ValidationResult.ValidResult;
                    }

                    if (!context.ReportValidationResult(validationResult, _fastReturnResult))
                    {
                        break;
                    }
                }
            }
        }

        public ResultTuple Result
        {
            get
            {
                if (_fastReturnResult is not null)
                {
                    return ResultTuple.Valid();
                }

                ValidationError curError = CreateValidationErrorWithLocation(ResultCode.NotFoundAnyValidatedArrayItem, _options.GenerateErrorMessages ? GetFailedContainsErrorMessage(_instance.ToString()) : string.Empty, ContainsKeywordName, _options.ValidationPathStack, _instance.Location);

                return ResultTuple.Invalid(curError);
            }
        }
    }

    private static ValidationError CreateValidationErrorWithLocation(ResultCode resultCode, string errorMessage, string locationName, ValidationPathStack validationPathStack, LinkedListBasedImmutableJsonPointer instanceLocation)
    {
        validationPathStack.PushRelativeLocation(locationName);
        var validationError = new ValidationError(resultCode, errorMessage, validationPathStack, locationName, instanceLocation);
        validationPathStack.PopRelativeLocation();

        return validationError;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return name == ContainsKeywordName 
            ? ContainsSchema 
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return ContainsSchema;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        if (_containsSchema is BodyJsonSchema bodyJsonSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _containsSchema = newSchema);
        }
    }

    public IReadOnlyCollection<int> GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<int>();
        }

        byte[] evaluatedItemsStatus = options.GetSubSchemaStatusBitmap(this, instance.Location);

        List<int>? evaluatedItems = null;

        int arrayLength = instance.GetArrayLength();
        for (int i = 0; i < arrayLength; i++)
        {
            if (BitmapHelper.IsBitSet(evaluatedItemsStatus, i))
            {
                evaluatedItems ??= new List<int>();
                evaluatedItems.Add(i);
            }
        }

        return evaluatedItems ?? (IReadOnlyCollection<int>)Array.Empty<int>();
    }
}