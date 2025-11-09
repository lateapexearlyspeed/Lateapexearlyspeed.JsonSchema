using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class TrueKeywordBuilderTests
{
    [Fact]
    public void Validate_IsTrue()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonTrue();
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("true");
        AssertValidationResult(validationResult,true);

        validationResult = jsonValidator.Validate("false");
        AssertValidationResult(validationResult, false, "Json kind not same, one is True, but another is False", LinkedListBasedImmutableJsonPointer.Empty);
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, LinkedListBasedImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);

        ValidationError? error = actualValidationResult.ValidationErrors.SingleOrDefault();

        Assert.Equal(expectedErrorMessage, error?.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, error?.InstanceLocation);
    }
}