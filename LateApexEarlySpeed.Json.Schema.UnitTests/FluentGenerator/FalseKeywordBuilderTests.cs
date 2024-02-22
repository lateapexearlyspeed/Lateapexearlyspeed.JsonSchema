using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class FalseKeywordBuilderTests
{
    [Fact]
    public void Validate_IsFalse()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonFalse();
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("false");
        AssertValidationResult(validationResult,true);

        validationResult = jsonValidator.Validate("true");
        AssertValidationResult(validationResult, false, "Json kind not same, one is False, but another is True", ImmutableJsonPointer.Empty);
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, ImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);
        Assert.Equal(expectedErrorMessage, actualValidationResult.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, actualValidationResult.InstanceLocation);
    }
}