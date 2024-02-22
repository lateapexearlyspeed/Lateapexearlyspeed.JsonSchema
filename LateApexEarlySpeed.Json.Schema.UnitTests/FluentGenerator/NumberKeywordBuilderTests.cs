using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests.FluentGenerator;

public class NumberKeywordBuilderTests
{
    [Fact]
    public void Validate_IsJsonNumber()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((long _) => true, _ => "bad msg").IsGreaterThan(long.MinValue)
            .IsLessThan(long.MaxValue).IsIn(new long[] { 1, 2, 3 }).MultipleOf(0.5).NotGreaterThan(long.MaxValue).NotLessThan(long.MinValue);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("\"1\"");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.String), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("[1]");
        AssertValidationResult(validationResult, false, GetInvalidTokenErrorMessage(InstanceType.Array), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasCustomValidation_WithLongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((long value) => value == 1, value => $"bad msg: {value}").IsGreaterThan(long.MinValue)
            .IsLessThan(long.MaxValue).IsIn(new long[] { 1, 2, 3 }).MultipleOf(0.5).NotGreaterThan(long.MaxValue).NotLessThan(long.MinValue);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, false, "bad msg: 2", ImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((long value) => value == 1, value => $"bad msg: {value}");
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("1.5");
        AssertValidationResult(validationResult, false, NumberCustomValidationKeyword<long>.ErrorMessageForTypeConvert("1.5"), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasCustomValidation_WithULongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((ulong value) => value == 1, value => $"bad msg: {value}").IsGreaterThan(long.MinValue)
            .IsLessThan(long.MaxValue).IsIn(new long[] { 1, 2, 3 }).MultipleOf(0.5).NotGreaterThan(long.MaxValue).NotLessThan(long.MinValue);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, false, "bad msg: 2", ImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((ulong value) => value == 1, value => $"bad msg: {value}");
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("1.5");
        AssertValidationResult(validationResult, false, NumberCustomValidationKeyword<ulong>.ErrorMessageForTypeConvert("1.5"), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_HasCustomValidation_WithDoubleTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double value) => Math.Abs(value - 1) < 0.0001, value => $"bad msg: {value}").IsGreaterThan(long.MinValue)
            .IsLessThan(long.MaxValue).IsIn(new long[] { 1, 2, 3 }).MultipleOf(0.5).NotGreaterThan(long.MaxValue).NotLessThan(long.MinValue);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, false, "bad msg: 2", ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_IsGreaterThan_WithDoubleTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").IsGreaterThan(1.0001);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1.00010001");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("5");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("123321");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(1, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("0.005");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(0.005, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-123");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(-123, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(long.MinValue, 1.0001), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_IsGreaterThan_WithLongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").IsGreaterThan(-100);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("-99.9999");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-95");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("123321");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-100");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(-100, -100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-100.0001");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(-100.0001, -100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-123");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(-123, -100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(long.MinValue, -100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, false, ExclusiveMinimumKeyword.ErrorMessage(double.MinValue, -100), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_IsLessThan_WithLongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").IsLessThan(100);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("99.99999");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-95");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(ulong.MaxValue, 100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(long.MaxValue, 100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(double.MaxValue, 100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("100");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(100, 100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("100.00001");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(100.00001, 100), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_IsLessThan_WithDoubleTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").IsLessThan(100.005);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("100.0049");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("100");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(ulong.MaxValue, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(long.MaxValue, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(double.MaxValue, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("101");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(101, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("100.005001");
        AssertValidationResult(validationResult, false, ExclusiveMaximumKeyword.ErrorMessage(100.005001, 100.005), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_NotGreaterThan_WithDoubleTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").NotGreaterThan(100.005);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("100.005");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("100.0049");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("100");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(ulong.MaxValue, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(long.MaxValue, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(double.MaxValue, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("101");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(101, 100.005), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("100.005001");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(100.005001, 100.005), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_NotGreaterThan_WithLongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").NotGreaterThan(100);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("99.99999");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("100");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-95");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(ulong.MaxValue, 100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(long.MaxValue, 100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(double.MaxValue, 100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("100.00001");
        AssertValidationResult(validationResult, false, MaximumKeyword.ErrorMessage(100.00001, 100), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_NotLessThan_WithDoubleTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").NotLessThan(1.0001);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1.00010001");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.0001");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("5");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("123321");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(1, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("0.005");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(0.005, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-123");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(-123, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(long.MinValue, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{ulong.MinValue}");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(ulong.MinValue, 1.0001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(double.MinValue, 1.0001), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_NotLessThan_WithLongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}").NotLessThan(-100);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("-99.9999");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-100");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-95");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("123321");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-100.0001");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(-100.0001, -100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-123");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(-123, -100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(long.MinValue, -100), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, false, MinimumKeyword.ErrorMessage(double.MinValue, -100), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_IsIn_WithDoubleTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}")
            .IsGreaterThan(long.MinValue).IsIn(new[] { 1, 2, 3.5 });
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("3.5");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("3.50001");
        AssertValidationResult(validationResult, false, EnumKeyword.ErrorMessage("3.50001"), ImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}")
            .IsGreaterThan(long.MinValue).IsIn(Array.Empty<double>());
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, false, EnumKeyword.ErrorMessage("1"), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_IsIn_WithLongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}")
            .IsGreaterThan(long.MinValue).IsIn(new long[] { 1, 2, -100 });
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-100");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("3.00001");
        AssertValidationResult(validationResult, false, EnumKeyword.ErrorMessage("3.00001"), ImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}")
            .IsGreaterThan(long.MinValue).IsIn(Array.Empty<long>());
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("1");
        AssertValidationResult(validationResult, false, EnumKeyword.ErrorMessage("1"), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_MultipleOf()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}")
            .IsGreaterThan(long.MinValue).MultipleOf(2);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-6");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2.5");
        AssertValidationResult(validationResult, false, MultipleOfKeyword.ErrorMessage(2.5, 2), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-2.5");
        AssertValidationResult(validationResult, false, MultipleOfKeyword.ErrorMessage(-2.5, 2), ImmutableJsonPointer.Empty);

        jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().HasCustomValidation((double _) => true, value => $"bad msg: {value}")
            .IsGreaterThan(long.MinValue).MultipleOf(1.25);
        jsonValidator = jsonSchemaBuilder.BuildValidator();

        validationResult = jsonValidator.Validate("0");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2.5");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-2.5");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("2.499");
        AssertValidationResult(validationResult, false, MultipleOfKeyword.ErrorMessage(2.499, 1.25), ImmutableJsonPointer.Empty);

        Assert.Throws<ArgumentOutOfRangeException>("multipleOf", () => new JsonSchemaBuilder().IsJsonNumber().MultipleOf(0));
        Assert.Throws<ArgumentOutOfRangeException>("multipleOf", () => new JsonSchemaBuilder().IsJsonNumber().MultipleOf(-1));
    }

    [Fact]
    public void Validate_Equal_WithDoubleTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().Equal(1.05);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("1.05");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("1.04999");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(1.05, 1.04999), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-10");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(1.05, -10), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(1.05, (double)long.MinValue), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MaxValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(1.05, double.MaxValue), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(1.05, (double)ulong.MaxValue), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_Equal_WithLongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().Equal(-123);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate("-123");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate("-123.001");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(-123, -123.001), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-10");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(-123, -10), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(-123, long.MinValue), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(-123, double.MinValue), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(-123, long.MaxValue), ImmutableJsonPointer.Empty);
    }

    [Fact]
    public void Validate_Equal_WithULongTypeArgument()
    {
        var jsonSchemaBuilder = new JsonSchemaBuilder();
        jsonSchemaBuilder.IsJsonNumber().Equal(ulong.MaxValue);
        JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

        ValidationResult validationResult = jsonValidator.Validate($"{ulong.MaxValue}");
        AssertValidationResult(validationResult, true);

        validationResult = jsonValidator.Validate($"{ulong.MaxValue - 0.0001}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(ulong.MaxValue, $"{ulong.MaxValue - 0.0001}"), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate("-123.5");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(ulong.MaxValue, -123.5), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MinValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(ulong.MaxValue, long.MinValue), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{double.MinValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(ulong.MaxValue, double.MinValue), ImmutableJsonPointer.Empty);

        validationResult = jsonValidator.Validate($"{long.MaxValue}");
        AssertValidationResult(validationResult, false, JsonInstanceElement.NumberNotSameMessageTemplate(ulong.MaxValue, long.MaxValue), ImmutableJsonPointer.Empty);
    }

    private static void AssertValidationResult(ValidationResult actualValidationResult, bool expectedValidStatus, string? expectedErrorMessage = null, ImmutableJsonPointer? expectedInstanceLocation = null)
    {
        Assert.Equal(expectedValidStatus, actualValidationResult.IsValid);
        Assert.Equal(expectedErrorMessage, actualValidationResult.ErrorMessage);
        Assert.Equal(expectedInstanceLocation, actualValidationResult.InstanceLocation);
    }

    private static string GetInvalidTokenErrorMessage(InstanceType actualType)
        => $"Expect type(s): '{InstanceType.Number}' but actual is '{actualType}'";
}