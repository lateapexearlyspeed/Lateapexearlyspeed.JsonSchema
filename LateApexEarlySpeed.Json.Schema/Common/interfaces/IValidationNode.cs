using System.Text.Json;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

public interface IValidationNode
{
    ValidationResult Validate(JsonElement instance, JsonSchemaOptions options);
}