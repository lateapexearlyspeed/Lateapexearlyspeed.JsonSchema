using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    private readonly JsonSchema _containsSchema;
    private readonly uint? _minContains;
    private readonly uint? _maxContains;

    public ArrayContainsValidator(JsonSchema containsSchema, uint? minContains, uint? maxContains)
    {
        _containsSchema = containsSchema;
        _minContains = minContains;
        _maxContains = maxContains;
    }

    public ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        if (!_minContains.HasValue && !_maxContains.HasValue)
        {
            return ValidateWithoutMinContainsAndMaxContains(instance, options);
        }

        if (_maxContains.HasValue)
        {
            return ValidateWithMaxContains(instance, options);
        }

        Debug.Assert(!_maxContains.HasValue);
        Debug.Assert(_minContains.HasValue);

        return ValidateWithoutMaxContains(instance, options);
    }

    private ValidationResult ValidateWithoutMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (_minContains == 0)
        {
            return ValidationResult.ValidResult;
        }

        int validatedItemCount = 0;

        Debug.Assert(_containsSchema is not null);

        foreach (JsonInstanceElement instanceItem in instance.EnumerateArray())
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

        return CreateFailedValidationResultWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMinContainsErrorMessage(), MinContainsKeywordName, options.ValidationPathStack, instance.Location);
    }

    private ValidationResult ValidateWithMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        int validatedItemCount = 0;

        Debug.Assert(_containsSchema is not null);

        foreach (JsonInstanceElement instanceItem in instance.EnumerateArray())
        {
            if (_containsSchema.Validate(instanceItem, options).IsValid)
            {
                validatedItemCount++;
            }

            if (validatedItemCount > _maxContains)
            {
                return CreateFailedValidationResultWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMaxContainsErrorMessage(), MaxContainsKeywordName, options.ValidationPathStack, instance.Location);
            }
        }

        if (_minContains.HasValue)
        {
            if (validatedItemCount < _minContains)
            {
                return CreateFailedValidationResultWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMinContainsErrorMessage(), MinContainsKeywordName, options.ValidationPathStack, instance.Location);
            }
        }
        else
        {
            if (validatedItemCount == 0)
            {
                return CreateFailedValidationResultWithLocation(ResultCode.NotFoundAnyValidatedArrayItem, GetFailedContainsErrorMessage(), ContainsKeywordName, options.ValidationPathStack, instance.Location);
            }
        }

        return ValidationResult.ValidResult;
    }

    private string GetFailedMaxContainsErrorMessage()
        => $"Validated array items count is greater than specified '{_maxContains}'";

    private string GetFailedMinContainsErrorMessage()
        => $"Validated array items count is less than specified '{_minContains}'";

    private string GetFailedContainsErrorMessage()
        => "Not found any validated array items";

    private ValidationResult ValidateWithoutMinContainsAndMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(_containsSchema is not null);

        foreach (JsonInstanceElement instanceItem in instance.EnumerateArray())
        {
            if (_containsSchema.Validate(instanceItem, options).IsValid)
            {
                return ValidationResult.ValidResult;
            }
        }

        return CreateFailedValidationResultWithLocation(ResultCode.NotFoundAnyValidatedArrayItem, GetFailedContainsErrorMessage(), ContainsKeywordName, options.ValidationPathStack, instance.Location);
    }

    private ValidationResult CreateFailedValidationResultWithLocation(ResultCode resultCode, string errorMessage, string locationName, ValidationPathStack validationPathStack, ImmutableJsonPointer instanceLocation)
    {
        validationPathStack.PushRelativeLocation(locationName);
        ValidationResult validationResult = ValidationResult.CreateFailedResult(resultCode, errorMessage, validationPathStack, locationName, instanceLocation);
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