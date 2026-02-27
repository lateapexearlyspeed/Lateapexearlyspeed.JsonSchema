using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class ObjectKeywordBuilderTests
{
    [Fact]
    public void Validate_IsJsonObject()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject();
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("{}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":1}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("[]");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Array), LinkedListBasedImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("null");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Null), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_SerializationEquivalent()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject().SerializationEquivalent(new {A = 1, B = new {C = "c"}});
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":1, "B":{"C":"c"}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":1, "B":{"C":"d"}}""");
        AssertValidationResult(validationResult, false, JsonInstanceElement.StringNotSameMessageTemplate("c", "d"), LinkedListBasedImmutableJsonPointer.Create("/B/C"));
    }

    [Fact]
    public void Validate_HasProperty()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject().HasProperty("B", b => b.IsJsonObject().HasProperty("C", b => b.IsJsonString()));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":1, "B":{"C":"c"}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":null, "B":{"C":1}}""");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Number, InstanceType.String), 
            LinkedListBasedImmutableJsonPointer.Create("/B/C"));

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject().HasProperty("A");
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("""{"A":1, "B":null}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":null}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"C":null}""");
        AssertValidationResult(validationResult, false, RequiredKeyword.ErrorMessage("A"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_ObjectHasProperty()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.ObjectHasProperty("A", a => a.IsJsonNumber().Equal(1))
            .HasProperty("B", b => b.IsJsonString().Equal("bbb"));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            {
              "A": 1,
              "B": "bbb"
            }
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            {
              "A": 2,
              "B": "bbb"
            }
            """);
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(1, 2), LinkedListBasedImmutableJsonPointer.Create("/A"));

        validationResult = jsonValidator.Validate("""
            {
              "A": 1,
              "B": "zzz"
            }
            """);
        AssertValidationResult(validationResult, false, JsonInstanceElement.StringNotSameMessageTemplate("bbb", "zzz"), LinkedListBasedImmutableJsonPointer.Create("/B"));
    }

    [Fact]
    public void Validate_HasCustomValidation_WithObjectArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.ObjectHasProperty("A", b => b.IsJsonObject().HasCustomValidation(typeof(TestClass), obj => ((TestClass)obj).A == "aaa", obj => ((TestClass)obj).A!));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":{"A":"aaa"}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":{"A":"bbb"}}""");
        AssertValidationResult(validationResult, false, "bbb", LinkedListBasedImmutableJsonPointer.Create("/A"));
    }

    [Fact]
    public void Validate_Equivalent()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.ObjectHasProperty("A", b => b.IsJsonObject().Equivalent("""{"B":[1, null, {"C":"aaa"}]}"""));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":{"B":[1, null, {"C":"aaa"}]}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":{"B":[1, null, {"C":"bbb"}]}}""");
        AssertValidationResult(validationResult, false, JsonInstanceElement.StringNotSameMessageTemplate("aaa", "bbb"), LinkedListBasedImmutableJsonPointer.Create("/A/B/2/C"));
    }

    [Fact]
    public void Validate_HasNoProperty()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.ObjectHasProperty("A", b => b.IsJsonObject().HasNoProperty("C"));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":{"B":null}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":{"B":null, "C":null}}""");
        AssertValidationResult(validationResult, false, NoPropertiesKeyword.ErrorMessage("C"), LinkedListBasedImmutableJsonPointer.Create("/A"));
    }

    private class TestClass
    {
        public string? A { get; set; }
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, LinkedListBasedImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);

        ValidationError? error = actualValidationResult.ValidationErrors.SingleOrDefault();

        Assert.Equal(expectedErrorMessage, error?.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, error?.InstanceLocation);
    }

    private static string GetInvalidTokenErrorMessage(InstanceType actualType, InstanceType expectedType = InstanceType.Object)
        => $"Expected type(s): '{expectedType}' but actual is '{actualType}'";
}
