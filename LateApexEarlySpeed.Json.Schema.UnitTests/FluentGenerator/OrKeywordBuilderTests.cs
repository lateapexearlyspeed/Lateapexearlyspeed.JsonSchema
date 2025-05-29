using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class OrKeywordBuilderTests
{
    [Fact]
    public void Validate_Or()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().Contains(b => b.Or(
            b => b.IsJsonString(),
            b => b.IsJsonNull()
            ));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();
        
        ValidationResult validationResult = jsonValidator.Validate("""[{}, "a"]""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""[{}, null]""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""[{}, 1]""");
        AssertValidationResult(validationResult, false, ArrayContainsValidator.GetFailedContainsErrorMessage("""[{}, 1]"""), ImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.ObjectHasProperty("A", b => b.Or(
            b => b.IsJsonString(),
            b => b.IsJsonNull()
        ));
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("""{"A": "a"}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A": null}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A": 1}""");
        AssertValidationResult(validationResult, false, AnyOfKeyword.ErrorMessage(), ImmutableJsonPointer.Create("/A"));
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, ImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);

        ValidationError? error = actualValidationResult.ValidationErrors.SingleOrDefault();

        Assert.Equal(expectedErrorMessage, error?.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, error?.InstanceLocation);
    }
}