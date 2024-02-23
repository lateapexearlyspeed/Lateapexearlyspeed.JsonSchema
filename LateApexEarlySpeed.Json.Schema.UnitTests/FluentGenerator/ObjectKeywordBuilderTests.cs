﻿using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
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
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Array), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("null");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Null), ImmutableJsonPointer.Empty);
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
        AssertValidationResult(validationResult, false, "String content not same, one is 'c', but another is 'd'", ImmutableJsonPointer.Create("/B/C"));
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
            ImmutableJsonPointer.Create("/B/C"));

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject().HasProperty("A");
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("""{"A":1, "B":null}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":null}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"C":null}""");
        AssertValidationResult(validationResult, false, RequiredKeyword.ErrorMessage("A"), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasCustomValidation_WithObjectArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject().HasProperty("A", b => b.IsJsonObject().HasCustomValidation(typeof(TestClass), obj => ((TestClass)obj).A == "aaa", obj => ((TestClass)obj).A!));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":{"A":"aaa"}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":{"A":"bbb"}}""");
        AssertValidationResult(validationResult, false, "bbb", ImmutableJsonPointer.Create("/A"));
    }

    [Fact]
    public void Validate_Equivalent()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject().HasProperty("A", b => b.IsJsonObject().Equivalent("""{"B":[1, null, {"C":"aaa"}]}"""));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":{"B":[1, null, {"C":"aaa"}]}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":{"B":[1, null, {"C":"bbb"}]}}""");
        AssertValidationResult(validationResult, false, "String content not same, one is 'aaa', but another is 'bbb'", ImmutableJsonPointer.Create("/A/B/2/C"));
    }

    [Fact]
    public void Validate_HasNoProperty()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonObject().HasProperty("A", b => b.IsJsonObject().HasNoProperty("C"));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""{"A":{"B":null}}""");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""{"A":{"B":null, "C":null}}""");
        AssertValidationResult(validationResult, false, NoPropertiesKeyword.ErrorMessage("C"), ImmutableJsonPointer.Create("/A"));
    }

    private class TestClass
    {
        public string? A { get; set; }
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, ImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);
        Assert.Equal(expectedErrorMessage, actualValidationResult.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, actualValidationResult.InstanceLocation);
    }

    private static string GetInvalidTokenErrorMessage(InstanceType actualType, InstanceType expectedType = InstanceType.Object)
        => $"Expect type(s): '{expectedType}' but actual is '{actualType}'";
}