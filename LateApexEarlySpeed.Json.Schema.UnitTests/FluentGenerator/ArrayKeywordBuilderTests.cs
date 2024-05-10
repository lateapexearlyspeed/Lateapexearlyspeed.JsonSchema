using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class ArrayKeywordBuilderTests
{
    [Fact]
    public void Validate_IsJsonArray()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasMaxLength(uint.MaxValue);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("[]");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("[1, {}, 3]");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("{}");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Object), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("null");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Null), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_SerializationEquivalent_WithObjectArray()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasMaxLength(uint.MaxValue)
            .SerializationEquivalent(new object?[]{-1, 1.5, null, "abc", new { A = 2, B = new{} }, new object[]{1, new{}} });
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [-1, 1.5, null, "abc", {"A":2, "B":{}}, [1, {}]]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1.5, null, "abc", {"A":2, "B":{}}, [1, {}]]
            """);
        AssertValidationResult(validationResult, false, "Array length not same, one is 6 but another is 5", ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("""
            [-1, 1.5, null, "abc", {"A":1111111111111, "B":{}}, [1, {}]]
            """);
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(2, 1111111111111), ImmutableJsonPointer.Create("/4/A"));

        validationResult = jsonValidator.Validate("""
            [-1, 1.5, null, "abc", {"A":2, "B":{}}, [1111111111, {}]]
            """);
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(1, 1111111111), ImmutableJsonPointer.Create("/5/0"));
    }

    [Fact]
    public void Validate_SerializationEquivalent_WithGenericTypeArray()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasMaxLength(uint.MaxValue)
            .SerializationEquivalent(new[] { 1, 2, 3});
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [1, 2, 3]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1, 2, 3, 4]
            """);
        AssertValidationResult(validationResult, false, "Array length not same, one is 3 but another is 4", ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasItems()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasMaxLength(uint.MaxValue)
            .HasItems(b => b.ObjectHasProperty("A", b => b.IsJsonString().HasMaxLength(5)));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"A":"aaaaa"}, {"A":""}]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [{"A":""}, {"A":"aaaaaa"}]
            """);
        AssertValidationResult(validationResult, false, MaxLengthKeyword.ErrorMessage(6, 5), ImmutableJsonPointer.Create("/1/A"));
    }

    [Fact]
    public void Validate_ArrayHasItems()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.ArrayHasItems(b => b.ObjectHasProperty("A", a => a.IsJsonString().HasMaxLength(5))).HasCustomValidation(_ => true, _ => "bad msg");
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"A":"aaaaa"}, {"A":""}]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [{"A":""}, {"A":"aaaaaa"}]
            """);
        AssertValidationResult(validationResult, false, MaxLengthKeyword.ErrorMessage(6, 5), ImmutableJsonPointer.Create("/1/A"));
    }

    [Fact]
    public void Validate_HasCollection()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasMaxLength(uint.MaxValue)
            .HasCollection(item => item.ObjectHasProperty("A", a => a.StringEqual("aaaaa")),
                item => item.ObjectHasProperty("A", a => a.IsJsonString().HasMaxLength(0)));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"A":"aaaaa"}, {"A":""}]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [{"A":"aaaaa"}, {"A":"x"}]
            """);
        AssertValidationResult(validationResult, false, MaxLengthKeyword.ErrorMessage(1, 0), ImmutableJsonPointer.Create("/1/A"));
    }

    [Fact]
    public void Validate_HasLength()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasLength(3);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [1, 2, 3]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1, 2]
            """);
        AssertValidationResult(validationResult, false, MinItemsKeyword.ErrorMessage(2, 3), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasMaxLength()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasMaxLength(3);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [1, 2, 3]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1, 2, 3, 4]
            """);
        AssertValidationResult(validationResult, false, MaxItemsKeyword.ErrorMessage(4, 3), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasMinLength()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasMinLength(3);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [1, 2, 3]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1, 2]
            """);
        AssertValidationResult(validationResult, false, MinItemsKeyword.ErrorMessage(2, 3), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasUniqueItems()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").HasUniqueItems();
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [1, 2, 3]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1, 2, 2]
            """);
        AssertValidationResult(validationResult, false, UniqueItemsKeyword.ErrorMessage("2", 1, 2), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("""
            [{"A": 1, "B": 2}, {"B": 2, "A": 1}]
            """);
        AssertValidationResult(validationResult, false, UniqueItemsKeyword.ErrorMessage("""{"A": 1, "B": 2}""", 0, 1), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasCustomValidation_WithGenericArray()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasUniqueItems().HasCustomValidation<TestClass>(
            array => array is not null && array.Length != 0 && array.All(element => element.A == 1), array => $"bad msg, length: {array?.Length}");
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"A":1}]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [{"A":0}]
            """);
        AssertValidationResult(validationResult, false,  "bad msg, length: 1", ImmutableJsonPointer.Empty);
    }

    private class TestClass
    {
        public int A { get; set; }
    }

    [Fact]
    public void Validate_HasCustomValidation_WithJsonElementArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasUniqueItems().HasCustomValidation(
            array => array.GetArrayLength() == 1, array => $"bad msg, length: {array.GetArrayLength()}");
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [1]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1, 2]
            """);
        AssertValidationResult(validationResult, false, "bad msg, length: 2", ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_Contains()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasUniqueItems().HasCustomValidation(
            _ => true, _ => "bad msg").Contains(b => b.ObjectHasProperty("A", b => b.IsJsonNumber().Equal(1)));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"A": 1}, 5]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [{"A": 2}, 5]
            """);
        AssertValidationResult(validationResult, false, ArrayContainsValidator.GetFailedContainsErrorMessage("""[{"A": 2}, 5]"""), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_ArrayContains()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.ArrayContains(b => b.ObjectHasProperty("A", a => a.IsJsonNumber().Equal(1)))
            .HasUniqueItems().HasCustomValidation(_ => true, _ => "bad msg");
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"A": 1}, 5]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [{"A": 2}, 5]
            """);
        AssertValidationResult(validationResult, false, ArrayContainsValidator.GetFailedContainsErrorMessage("""[{"A": 2}, 5]"""), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_NotContains()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasUniqueItems().HasCustomValidation(
            _ => true, _ => "bad msg").NotContains(b => b.ObjectHasProperty("A", b => b.IsJsonNumber().Equal(1)));
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"A": 2}, 5]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [5, {"A": 1}]
            """);
        AssertValidationResult(validationResult, false, NotContainsKeyword.ErrorMessage("""{"A": 1}"""), ImmutableJsonPointer.Create("/1"));
    }

    [Fact]
    public void Validate_Equivalent()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasUniqueItems().HasCustomValidation(
            _ => true, _ => "bad msg").Equivalent("""[{"A":1, "B":2}]""");
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            [{"B":2, "A":1}]
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [{"B":2, "C":1}]
            """);
        AssertValidationResult(validationResult, false,  "Properties not match, one has property:A but another not", ImmutableJsonPointer.Create("/0"));
    }

    [Fact]
    public void Validate_Empty()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonArray().HasCustomValidation(_ => true, _ => "bad msg").Empty();
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("""
            []
            """);
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("""
            [1]
            """);
        AssertValidationResult(validationResult, false, MaxItemsKeyword.ErrorMessage(1, 0), ImmutableJsonPointer.Empty);
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, ImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);
        Assert.Equal(expectedErrorMessage, actualValidationResult.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, actualValidationResult.InstanceLocation);
    }

    private static string GetInvalidTokenErrorMessage(InstanceType actualType)
        => $"Expect type(s): '{InstanceType.Array}' but actual is '{actualType}'";
}