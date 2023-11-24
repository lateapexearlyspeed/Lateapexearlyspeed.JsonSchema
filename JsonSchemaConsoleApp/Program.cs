using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;

namespace JsonSchemaConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string jsonSchema = File.ReadAllText("schema.json");
            string instance = File.ReadAllText("instance.json");

            var jsonValidator = new JsonValidator(jsonSchema);

            while (true)
            {
                ValidationResult result = jsonValidator.Validate(instance);
            }
        }
    }
}