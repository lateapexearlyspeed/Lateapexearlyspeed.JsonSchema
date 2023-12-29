using System.Globalization;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Generator;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace JsonSchemaConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator<TestClass>();

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