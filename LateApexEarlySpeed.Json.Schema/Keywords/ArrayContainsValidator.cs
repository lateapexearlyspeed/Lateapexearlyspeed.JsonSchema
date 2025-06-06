using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class ArrayContainsValidator : ISchemaContainerValidationNode
{
    public const string ContainsKeywordName = "contains";
    public const string MaxContainsKeywordName = "maxContains";
    public const string MinContainsKeywordName = "minContains";

    public JsonSchema ContainsSchema { get; }
    public uint? MinContains { get; }
    public uint? MaxContains { get; }

    public ArrayContainsValidator(JsonSchema containsSchema, uint? minContains, uint? maxContains)
    {
        containsSchema.Name = ContainsKeywordName;
        ContainsSchema = containsSchema;

        MinContains = minContains;
        MaxContains = maxContains;
    }

    public ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        if (!MinContains.HasValue && !MaxContains.HasValue)
        {
            return ValidateWithoutMinContainsAndMaxContains(instance, options);
        }

        if (MaxContains.HasValue)
        {
            return ValidateWithMaxContains(instance, options);
        }

        Debug.Assert(!MaxContains.HasValue);
        Debug.Assert(MinContains.HasValue);

        return ValidateWithoutMaxContains(instance, options);
    }

    private ValidationResult ValidateWithoutMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(MinContains.HasValue);

        if (MinContains == 0)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new WithoutMaxContainsValidator(this, instance, options), options.OutputFormat);
    }

    private class WithoutMaxContainsValidator : IValidator
    {
        private readonly ArrayContainsValidator _arrayContainsValidator;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        private int _validatedItemCount;

        public WithoutMaxContainsValidator(ArrayContainsValidator arrayContainsValidator, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _arrayContainsValidator = arrayContainsValidator;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            Debug.Assert(_arrayContainsValidator.ContainsSchema is not null);

            foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
            {
                ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                if (validationResult.IsValid)
                {
                    _validatedItemCount++;
                }

                yield return validationResult;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            if (_validatedItemCount >= _arrayContainsValidator.MinContains)
            {
                validationResult = ValidationResult.ValidResult;
                return true;
            }

            validationResult = null;
            return false;
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

                ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMinContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MinContains.Value), MinContainsKeywordName, _options.ValidationPathStack, _instance.Location);
                return ResultTuple.Invalid(error);
            }
        }
    }

    private ValidationResult ValidateWithMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return ValidationResultsComposer.Compose(new WithMaxContainsValidator(this, instance, options), options.OutputFormat);
    }

    private class WithMaxContainsValidator : IValidator
    {
        private readonly ArrayContainsValidator _arrayContainsValidator;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        private int _validatedItemCount;

        public WithMaxContainsValidator(ArrayContainsValidator arrayContainsValidator, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _arrayContainsValidator = arrayContainsValidator;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            Debug.Assert(_arrayContainsValidator.ContainsSchema is not null);

            foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
            {
                ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                if (validationResult.IsValid)
                {
                    _validatedItemCount++;
                }

                yield return validationResult;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            if (_validatedItemCount > _arrayContainsValidator.MaxContains)
            {
                ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMaxContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MaxContains.Value), MaxContainsKeywordName, _options.ValidationPathStack, _instance.Location);
                validationResult = ValidationResult.SingleErrorFailedResult(error);

                return true;
            }

            validationResult = null;
            return false;
        }

        public ResultTuple Result
        {
            get
            {
                if (_validatedItemCount > _arrayContainsValidator.MaxContains)
                {
                    ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMaxContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MaxContains.Value), MaxContainsKeywordName, _options.ValidationPathStack, _instance.Location);
                    return ResultTuple.Invalid(error);
                }

                if (_arrayContainsValidator.MinContains.HasValue)
                {
                    if (_validatedItemCount < _arrayContainsValidator.MinContains)
                    {
                        ValidationError error = CreateValidationErrorWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMinContainsErrorMessage(_instance.ToString(), _arrayContainsValidator.MinContains.Value), MinContainsKeywordName, _options.ValidationPathStack, _instance.Location);

                        return ResultTuple.Invalid(error);
                    }
                }
                else
                {
                    if (_validatedItemCount == 0)
                    {
                        ValidationError error = CreateValidationErrorWithLocation(ResultCode.NotFoundAnyValidatedArrayItem, GetFailedContainsErrorMessage(_instance.ToString()), ContainsKeywordName, _options.ValidationPathStack, _instance.Location);

                        return ResultTuple.Invalid(error);
                    }
                }

                return ResultTuple.Valid();
            }
        }
    }

    internal static string GetFailedMaxContainsErrorMessage(string instanceJson, uint maxContains)
        => $"Validated array items count is greater than specified '{maxContains}', array instance: {instanceJson}";

    internal static string GetFailedMinContainsErrorMessage(string instanceJson, uint minContains)
        => $"Validated array items count is less than specified '{minContains}', array instance: {instanceJson}";

    internal static string GetFailedContainsErrorMessage(string instanceJson)
        => $"Not found any validated array items, array instance: {instanceJson}";

    private ValidationResult ValidateWithoutMinContainsAndMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return ValidationResultsComposer.Compose(new WithoutMinContainsAndMaxContainsValidator(this, instance, options), options.OutputFormat);
    }

    private class WithoutMinContainsAndMaxContainsValidator : IValidator
    {
        private readonly ArrayContainsValidator _arrayContainsValidator;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        private ValidationResult? _fastReturnResult;

        public WithoutMinContainsAndMaxContainsValidator(ArrayContainsValidator arrayContainsValidator, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _arrayContainsValidator = arrayContainsValidator;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            Debug.Assert(_arrayContainsValidator.ContainsSchema is not null);

            foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
            {
                ValidationResult validationResult = _arrayContainsValidator.ContainsSchema.Validate(instanceItem, _options);
                if (validationResult.IsValid)
                {
                    _fastReturnResult = ValidationResult.ValidResult;
                }

                yield return validationResult;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            validationResult = _fastReturnResult;

            return _fastReturnResult is not null;
        }

        public ResultTuple Result
        {
            get
            {
                if (_fastReturnResult is not null)
                {
                    return ResultTuple.Valid();
                }

                ValidationError curError = CreateValidationErrorWithLocation(ResultCode.NotFoundAnyValidatedArrayItem, GetFailedContainsErrorMessage(_instance.ToString()), ContainsKeywordName, _options.ValidationPathStack, _instance.Location);

                return ResultTuple.Invalid(curError);
            }
        }
    }

    private static ValidationError CreateValidationErrorWithLocation(ResultCode resultCode, string errorMessage, string locationName, ValidationPathStack validationPathStack, ImmutableJsonPointer instanceLocation)
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
}