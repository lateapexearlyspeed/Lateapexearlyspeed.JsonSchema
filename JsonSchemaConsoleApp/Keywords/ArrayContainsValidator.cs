using System.Diagnostics;
using System.Text.Json;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

internal class ArrayContainsValidator : ISchemaContainerValidationNode
{
    public const string ContainsKeywordName = "contains";
    public const string MaxContainsKeywordName = "maxContains";
    public const string MinContainsKeywordName = "minContains";

    private readonly JsonSchema _containsSchema;
    private readonly uint? _minContains;
    private readonly uint? _maxContains;

    public ArrayContainsValidator(JsonSchema containsSchema, uint? minContains, uint? maxContains)
    {
        _containsSchema = containsSchema;
        _minContains = minContains;
        _maxContains = maxContains;
    }

    public ValidationResult Validate(JsonElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        if (!_minContains.HasValue && !_maxContains.HasValue)
        {
            return ValidateWithMinContainsAndMaxContains(instance, options);
        }

        if (_maxContains.HasValue)
        {
            return ValidateWithMaxContains(instance, options);
        }

        Debug.Assert(!_maxContains.HasValue);
        Debug.Assert(_minContains.HasValue);

        return ValidateWithoutMaxContains(instance, options);
    }

    private ValidationResult ValidateWithoutMaxContains(JsonElement instance, JsonSchemaOptions options)
    {
        if (_minContains == 0)
        {
            return ValidationResult.ValidResult;
        }

        int validatedItemCount = 0;

        Debug.Assert(_containsSchema is not null);

        foreach (JsonElement instanceItem in instance.EnumerateArray())
        {
            if (_containsSchema.Validate(instanceItem, options).IsValid)
            {
                validatedItemCount++;
            }

            if (validatedItemCount >= _minContains)
            {
                return ValidationResult.ValidResult;
            }
        }

        return CreateFailedValidationResultWithLocation(ResultCode.ContainsOutOfRange, MinContainsKeywordName, options.ValidationPathStack);
    }

    private ValidationResult ValidateWithMaxContains(JsonElement instance, JsonSchemaOptions options)
    {
        int validatedItemCount = 0;

        Debug.Assert(_containsSchema is not null);

        foreach (JsonElement instanceItem in instance.EnumerateArray())
        {
            if (_containsSchema.Validate(instanceItem, options).IsValid)
            {
                validatedItemCount++;
            }

            if (validatedItemCount > _maxContains)
            {
                return CreateFailedValidationResultWithLocation(ResultCode.ContainsOutOfRange, MaxContainsKeywordName, options.ValidationPathStack);
            }
        }

        if (_minContains.HasValue && validatedItemCount < _minContains)
        {
            return CreateFailedValidationResultWithLocation(ResultCode.ContainsOutOfRange, MinContainsKeywordName, options.ValidationPathStack);
        }

        return ValidationResult.ValidResult;
    }

    private ValidationResult ValidateWithMinContainsAndMaxContains(JsonElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(_containsSchema is not null);

        foreach (JsonElement instanceItem in instance.EnumerateArray())
        {
            if (_containsSchema.Validate(instanceItem, options).IsValid)
            {
                return ValidationResult.ValidResult;
            }
        }

        return CreateFailedValidationResultWithLocation(ResultCode.NotFoundValidatedItem, ContainsKeywordName, options.ValidationPathStack);
    }

    private ValidationResult CreateFailedValidationResultWithLocation(ResultCode resultCode, string locationName, ValidationPathStack validationPathStack)
    {
        validationPathStack.PushRelativeLocation(locationName);
        ValidationResult validationResult = ValidationResult.CreateFailedResult(resultCode, validationPathStack);
        validationPathStack.PopRelativeLocation();

        return validationResult;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return name == ContainsKeywordName 
            ? _containsSchema 
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return _containsSchema;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}