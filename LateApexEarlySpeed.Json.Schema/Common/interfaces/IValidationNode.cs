using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

public interface IValidationNode
{
    ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options);
}