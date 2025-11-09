using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class StringKeywordBuilderTests
{
    [Fact]
    public void Validate_IsJsonString()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(instance => instance.StartsWith(string.Empty), instance => instance)
            .HasMaxLength(1000).HasMinLength(0).HasPattern(".*").IsIn(new[] { string.Empty, "abc", "def" });

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"abc\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Number), LinkedListBasedImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("{}");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Object), LinkedListBasedImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("null");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Null), LinkedListBasedImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("\"abc\"");
        AssertValidationResult(validationResult, true);
    }

    [Fact]
    public void Validate_HasCustomValidation()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(instance => instance.StartsWith("abc"), instance => $"{instance} is bad one")
            .HasMaxLength(1000).HasMinLength(0).HasPattern(".*").IsIn(new[] { string.Empty, "abc", "def" });

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"abc\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"def\"");
        AssertValidationResult(validationResult, false, "def is bad one", LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasMaxLength()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(instance => instance.StartsWith(string.Empty), instance => $"{instance} is bad one")
            .HasMaxLength(2).HasMinLength(0).HasPattern(".*").IsIn(new[] { "a", "ab", "def" });

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"ab\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"a\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"def\"");
        AssertValidationResult(validationResult, false, MaxLengthKeyword.ErrorMessage(3, 2), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasMinLength()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(instance => instance.StartsWith(string.Empty), instance => $"{instance} is bad one")
            .HasMaxLength(1000).HasMinLength(2).HasPattern(".*").IsIn(new[] { "a", "ab", "def" });

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"def\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"ab\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"a\"");
        AssertValidationResult(validationResult, false, MinLengthKeyword.ErrorMessage(1, 2), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasPattern()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(instance => instance.StartsWith(string.Empty), instance => $"{instance} is bad one")
            .HasMaxLength(1000).HasMinLength(0).HasPattern(".a.").IsIn(new[] { "a", "ab", "1a2" });

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"1a2\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"a\"");
        AssertValidationResult(validationResult, false, PatternKeyword.ErrorMessage(".a.", "a"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_StringHasPattern()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.StringHasPattern(".a.").HasMaxLength(1000).HasMinLength(0).IsIn(new[] { "a", "ab", "1a2" });

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"1a2\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"a\"");
        AssertValidationResult(validationResult, false, PatternKeyword.ErrorMessage(".a.", "a"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_StartsWith()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(_ => true, _ => string.Empty).StartsWith("abc");

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"abc111\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"abC111\"");
        AssertValidationResult(validationResult, false, StringKeywordBuilder.StartsWithErrorMessage("abC111", "abc"), LinkedListBasedImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(_ => true, _ => string.Empty).StartsWith("abc", StringComparison.OrdinalIgnoreCase);

        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("\"abc111\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"ABC111\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"def111\"");
        AssertValidationResult(validationResult, false, StringKeywordBuilder.StartsWithErrorMessage("def111", "abc"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_EndsWith()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(_ => true, _ => string.Empty).EndsWith("abc");

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"111abc\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"111abC\"");
        AssertValidationResult(validationResult, false, StringKeywordBuilder.EndsWithErrorMessage("111abC", "abc"), LinkedListBasedImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(_ => true, _ => string.Empty).EndsWith("abc", StringComparison.OrdinalIgnoreCase);

        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("\"111abc\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"111ABC\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"111def\"");
        AssertValidationResult(validationResult, false, StringKeywordBuilder.EndsWithErrorMessage("111def", "abc"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_NotMatch()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().NotMatch(".a.");

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"a\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"1a2\"");
        AssertValidationResult(validationResult, false, NotKeyword.ErrorMessage("1a2"), LinkedListBasedImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().NotMatch(".a.").NotMatch(".b.");

        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("\"c\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"1a2\"");
        AssertValidationResult(validationResult, false, NotKeyword.ErrorMessage("1a2"), LinkedListBasedImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("\"1b2\"");
        AssertValidationResult(validationResult, false, NotKeyword.ErrorMessage("1b2"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_IsIn()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().HasCustomValidation(instance => instance.StartsWith(string.Empty), instance => $"{instance} is bad one")
            .HasMaxLength(1000).HasMinLength(0).HasPattern(".*").IsIn(new[] { "a", "ab", "1a2" });

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"1a2\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"a\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"abc\"");
        AssertValidationResult(validationResult, false, EnumKeyword.ErrorMessage("abc"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_Equal()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().Equal("abc");

        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"abc\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"def\"");
        AssertValidationResult(validationResult, false, JsonInstanceElement.StringNotSameMessageTemplate("abc", "def"), LinkedListBasedImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonString().Equal(string.Empty);
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("\"\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"def\"");
        AssertValidationResult(validationResult, false, JsonInstanceElement.StringNotSameMessageTemplate("", "def"), LinkedListBasedImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_StringEqual()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.StringEqual("abc").IsIn(new[] { "abc", "def" });
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("\"abc\"");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"def\"");
        AssertValidationResult(validationResult, false, JsonInstanceElement.StringNotSameMessageTemplate("abc", "def"), LinkedListBasedImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("null");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Null), LinkedListBasedImmutableJsonPointer.Empty);
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, LinkedListBasedImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);

        ValidationError? error = actualValidationResult.ValidationErrors.SingleOrDefault();

        Assert.Equal(expectedErrorMessage, error?.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, error?.InstanceLocation);
    }

    private static string GetInvalidTokenErrorMessage(InstanceType actualType)
        => $"Expect type(s): '{InstanceType.String}' but actual is '{actualType}'";
}


