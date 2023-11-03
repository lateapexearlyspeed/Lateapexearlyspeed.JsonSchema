using System.Text.Json;

namespace JsonSchemaConsoleApp;

public interface IValidationNode
{
    ValidationResult Validate(JsonElement instance, JsonSchemaOptions options);
}