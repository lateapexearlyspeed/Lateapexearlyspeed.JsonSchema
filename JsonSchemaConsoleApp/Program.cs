using System.Text.Json;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.Generator;

namespace JsonSchemaConsoleApp
{
    public class TestDto
    {
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            JsonSchemaBuilder builder = new JsonSchemaBuilder();

            builder.IsJsonString().Equal("abc").IsIn(new string[] { "abc", "def" }).HasMaxLength(10).HasMinLength(3).HasPattern(".*a").HasCustomValidation(s => s.StartsWith('a'), s => s);
            builder.IsJsonNumber().Equal(3d).Equal(1).Equal(1UL).IsIn(new double[] { 1, 2, 3 }).IsGreaterThan(1).IsLessThan(10).NotGreaterThan(11).NotLessThan(0).MultipleOf(1.5)
                .HasCustomValidation((double d) => Math.Abs(d - 1.5) < 0.001, d => "").HasCustomValidation((long ll) => true, ll => "");
            builder.IsJsonObject().Equivalent("""{"A": "a", "B": 1.0001}""").SerializationEquivalent(new { A = "a", B = 1.0001 }).HasNoProperty("aaa").HasProperty("A").HasProperty("A", b => b.IsJsonString()).HasProperty("B")
                .HasCustomValidation<TestDto>(tc => true, tc => "").HasCustomValidation(typeof(TestDto), tc => true, tc => "").HasCustomValidation(element => true, element => "");
            builder.IsJsonArray().Equivalent("[]").NotContains(b => b.IsJsonString()).Contains(b => b.IsJsonString()).SerializationEquivalent(new object[] { }).HasItems(b => b.IsJsonString()).HasLength(8).HasMaxLength(10).HasMinLength(1).HasUniqueItems().HasCustomValidation<TestDto>(array => true, array => "").HasCustomValidation(element => true, element => "");
            builder.IsJsonNull();
            builder.NotJsonNull();
            builder.IsJsonBoolean();
            builder.IsJsonTrue();
            builder.IsJsonFalse();
            // builder.IsDateTimeOffset().Equal(DateTimeOffset.UtcNow).Before(DateTimeOffset.UtcNow).After(DateTimeOffset.UtcNow).HasCustomValidation(dt => true, dt => "");
            // builder.IsDateTime().Equal(DateTime.UtcNow).Before(DateTime.UtcNow).After(DateTime.UtcNow).HasCustomValidation(dt => true, dt => "");
            // builder.IsGuid();
            builder.Or(b => b.IsJsonObject(), b => b.IsJsonString(), b => b.IsJsonNull());

            JsonValidator validator = builder.BuildValidator();
            ValidationResult ret = validator.Validate("""{"A": "a", "B": 1.0002}""");

            // JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator<TestClass>();

            string jsonSchema = File.ReadAllText("schema.json");
            string instance = File.ReadAllText("instance.json");

            var jsonValidator = new JsonValidator(jsonSchema);
            ValidationResult validationResult = jsonValidator.Validate(instance);

            if (validationResult.IsValid)
            {
                Console.WriteLine("good");
            }
            else
            {
                Console.WriteLine($"Failed keyword: {validationResult.Keyword}");
                Console.WriteLine($"ResultCode: {validationResult.ResultCode}");
                Console.WriteLine($"Error message: {validationResult.ErrorMessage}");
                Console.WriteLine($"Failed instance location: {validationResult.InstanceLocation}");
                Console.WriteLine($"Failed relative keyword location: {validationResult.RelativeKeywordLocation}");
                Console.WriteLine($"Failed schema resource base uri: {validationResult.SchemaResourceBaseUri}");
            }

            while (true)
            {
                ValidationResult result = jsonValidator.Validate(instance);
            }
        }
    }
}