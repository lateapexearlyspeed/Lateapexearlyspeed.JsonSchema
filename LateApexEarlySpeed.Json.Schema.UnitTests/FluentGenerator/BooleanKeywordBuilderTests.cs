using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class BooleanKeywordBuilderTests
{
    [Fact]
    public void Validate_IsJsonBoolean()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonBoolean();

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("true");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("false");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"false\"");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.String), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Number), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("{}");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Object), ImmutableJsonPointer.Empty);
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, ImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);
        Assert.Equal(expectedErrorMessage, actualValidationResult.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, actualValidationResult.InstanceLocation);
    }

    private static string GetInvalidTokenErrorMessage(InstanceType actualType)
        => $"Expect type(s): '{InstanceType.Boolean}' but actual is '{actualType}'";
}