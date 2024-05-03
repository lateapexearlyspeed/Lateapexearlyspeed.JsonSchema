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

    public JsonSchema ContainsSchema { get; }
    public uint? MinContains { get; }
    public uint? MaxContains { get; }

    public ArrayContainsValidator(JsonSchema containsSchema, uint? minContains, uint? maxContains)
    {
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
        if (MinContains == 0)
        {
            return ValidationResult.ValidResult;
        }

        int validatedItemCount = 0;

        Debug.Assert(ContainsSchema is not null);

        foreach (JsonInstanceElement instanceItem in instance.EnumerateArray())
        {
            if (ContainsSchema.Validate(instanceItem, options).IsValid)
            {
                validatedItemCount++;
            }

            if (validatedItemCount >= MinContains)
            {
                return ValidationResult.ValidResult;
            }
        }

        return CreateFailedValidationResultWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMinContainsErrorMessage(), MinContainsKeywordName, options.ValidationPathStack, instance.Location);
    }

    private ValidationResult ValidateWithMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        int validatedItemCount = 0;

        Debug.Assert(ContainsSchema is not null);

        foreach (JsonInstanceElement instanceItem in instance.EnumerateArray())
        {
            if (ContainsSchema.Validate(instanceItem, options).IsValid)
            {
                validatedItemCount++;
            }

            if (validatedItemCount > MaxContains)
            {
                return CreateFailedValidationResultWithLocation(ResultCode.ValidatedArrayItemsCountOutOfRange, GetFailedMaxContainsErrorMessage(), MaxContainsKeywordName, options.ValidationPathStack, instance.Location);
            }
        }

        if (MinContains.HasValue)
        {
            if (validatedItemCount < MinContains)
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
        => $"Validated array items count is greater than specified '{MaxContains}'";

    private string GetFailedMinContainsErrorMessage()
        => $"Validated array items count is less than specified '{MinContains}'";

    private string GetFailedContainsErrorMessage()
        => "Not found any validated array items";

    private ValidationResult ValidateWithoutMinContainsAndMaxContains(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        Debug.Assert(ContainsSchema is not null);

        foreach (JsonInstanceElement instanceItem in instance.EnumerateArray())
        {
            if (ContainsSchema.Validate(instanceItem, options).IsValid)
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