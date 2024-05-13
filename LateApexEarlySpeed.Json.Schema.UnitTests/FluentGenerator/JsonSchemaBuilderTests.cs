using System.Text.Json;
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

        [Fact]
        public void GetStandardJsonSchemaText_BuildObjectWithStandardKeywords_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();
            
            // Object
            builder.ObjectHasProperty("a", a => a.IsJsonString()).HasProperty("b").HasProperty("c", c => c.IsJsonString()).Equivalent("null").SerializationEquivalent(new { A = 1 });
            JsonValidator jsonValidator = builder.BuildValidator();

            // Should no exception
            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.IsJsonObject().HasProperty("a", a => a.IsJsonString());
            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildArrayWithStandardKeywords_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.ArrayContains(a => a.IsJsonString()).Contains(a => a.IsJsonString()).Single().Single(i => i.IsJsonString()).Equivalent("null").HasItems(a => a.IsJsonString()).HasLength(1).HasMaxLength(2).Empty().NotEmpty()
                .HasMinLength(1).HasUniqueItems().SerializationEquivalent(new object?[] { new { A = 1 } }).SerializationEquivalent(new[] { 1 })
                .HasCollection(b => b.IsJsonString());

            JsonValidator jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.ArrayHasItems(a => a.IsJsonString());

            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.IsJsonArray().HasItems(a => a.IsJsonString());

            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildNumberWithStandardKeywords_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.IsJsonNumber().Equal(1.5d).Equal(100L).Equal(1000UL).IsGreaterThan(1.5d).IsGreaterThan(100L).IsIn(new[] { 1.5d }).IsIn(new[] { 100L })
                .IsLessThan(1.5d).IsLessThan(100L).NotGreaterThan(1.5d).NotGreaterThan(100L).NotLessThan(1.5d).NotLessThan(100L).MultipleOf(1.5d);

            JsonValidator jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildStringWithStandardKeywords_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.StringEqual("a").Equal("a").HasMaxLength(100).HasMinLength(1).HasPattern("a.b").IsIn(new[] { "a" });

            JsonValidator jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.StringHasPattern("a.b");

            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.IsJsonString().Equal("a");

            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildWithEquivalent_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.Equivalent("""{"A": 1}""");

            JsonValidator jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildWithBoolean_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.IsJsonBoolean();

            JsonValidator jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.IsJsonTrue();

            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.IsJsonFalse();

            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildWithNull_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.IsJsonNull();

            JsonValidator jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();

            builder = new JsonSchemaBuilder();

            builder.NotJsonNull();

            jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildWithOr_ShouldNotThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.Or(a => a.IsJsonString(), b => b.IsJsonNull());

            JsonValidator jsonValidator = builder.BuildValidator();

            jsonValidator.GetStandardJsonSchemaText();
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildObjectWithExtendedKeywords_ShouldThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.IsJsonObject().HasCustomValidation((JsonElement _) => true, _ => "");

            JsonValidator jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonObject().HasCustomValidation((int _) => true, _ => "");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonObject().HasCustomValidation(typeof(int), _ => true, _ => "");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonObject().HasNoProperty("A");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildArrayWithExtendedKeywords_ShouldThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.IsJsonArray().HasCustomValidation((JsonElement _) => true, _ => "");

            JsonValidator jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonArray().HasCustomValidation<int>(_ => true, _ => "");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonArray().NotContains(item => item.IsJsonString());

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildStringWithExtendedKeywords_ShouldThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.IsJsonString().HasCustomValidation(_ => true, _ => "");

            JsonValidator jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonString().StartsWith("abc");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonString().EndsWith("abc");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());
        }

        [Fact]
        public void GetStandardJsonSchemaText_BuildNumberWithExtendedKeywords_ShouldThrowException()
        {
            var builder = new JsonSchemaBuilder();

            builder.IsJsonNumber().HasCustomValidation((double _) => true, _ => "");

            JsonValidator jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonNumber().HasCustomValidation((long _) => true, _ => "");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());

            builder = new JsonSchemaBuilder();

            builder.IsJsonNumber().HasCustomValidation((ulong _) => true, _ => "");

            jsonValidator = builder.BuildValidator();

            Assert.Throws<NotSupportedException>(() => jsonValidator.GetStandardJsonSchemaText());
        }
    }
}
