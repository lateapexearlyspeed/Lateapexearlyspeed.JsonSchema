using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class NullKeywordBuilderTests
{
    [Fact]
    public void Validate_IsJsonNull()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNull();

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("null");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"null\"");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.String, InstanceType.Null), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_NotJsonNull()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.NotJsonNull();

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"null\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"abc\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("{}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("null");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Null, InstanceType.Object, InstanceType.Array, InstanceType.Boolean, InstanceType.Number, InstanceType.String), LinkedListBasedImmutableJsonPointer.Empty);
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, LinkedListBasedImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);

        ValidationError? error = actualValidationResult.ValidationErrors.SingleOrDefault();

        Assert.Equal(expectedErrorMessage, error?.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, error?.InstanceLocation);
    }

    private static string GetInvalidTokenErrorMessage(InstanceType actualType, params InstanceType[] expectedTypes)
        => $"Expect type(s): '{string.Join('|', expectedTypes)}' but actual is '{actualType}'";
}