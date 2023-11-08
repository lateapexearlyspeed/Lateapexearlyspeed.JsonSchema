using System.ComponentModel.Design;
using System.Security.AccessControl;

namespace JsonSchemaConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string jsonSchema = File.ReadAllText("schema.json");
            string instance = File.ReadAllText("instance.json");

            // ValidationKeywordRegistry.AddKeyword(typeof(TypeKeyword));

            var jsonValidator = new JsonValidator(jsonSchema);
            string jsonSchema2 = File.ReadAllText("schema2.json");
            jsonValidator.AddExternalDocument(jsonSchema2);

            ValidationResult result = jsonValidator.Validate(instance);
        }
    }
}