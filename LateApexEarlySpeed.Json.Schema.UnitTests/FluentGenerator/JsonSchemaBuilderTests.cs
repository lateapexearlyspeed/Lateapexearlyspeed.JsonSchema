using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.JInstance;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator
{
    public class JsonSchemaBuilderTests
    {
        [Fact]
        public void Equivalent()
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            jsonSchemaBuilder.Equivalent("""
                {
                  "a": 1,
                  "b": null,
                  "c": [1, 2, "a", null]
                }
                """);
            JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

            ValidationResult validationResult = jsonValidator.Validate("""
                {
                  "a": 1,
                  "c": [1, 2, "a", null],
                  "b": null
                }
                """);
            AssertValidationResult(validationResult, true);

            validationResult = jsonValidator.Validate("""
                {
                  "a": 1,
                  "c": [1, 2, "b", null],
                  "b": null
                }
                """);
            AssertValidationResult(validationResult, false, JsonInstanceElement.StringNotSameMessageTemplate("a", "b"), ImmutableJsonPointer.Create("/c/2"));
        }

        private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, ImmutableJsonPointer? expectedInstanceLocation = null)
        {
            Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);
            Assert.Equal(expectedErrorMessage, actualValidationResult.ErrorMessage);
            Assert.Equal(expectedInstanceLocation, actualValidationResult.InstanceLocation);
        }
    }
}
