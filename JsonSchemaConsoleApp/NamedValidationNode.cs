using System.Text.Json;

namespace JsonSchemaConsoleApp;

public abstract class NamedValidationNode : IValidationNode
{
    public string? Name { get; set; }

    public ValidationResult Validate(JsonElement instance, JsonSchemaOptions options)
    {
        if (Name is not null)
        {
            options.ValidationPathStack.PushRelativeLocation(Name);
        }
        
        ValidationResult validationResult = ValidateCore(instance, options);

        if (Name is not null)
        {
            options.ValidationPathStack.PopRelativeLocation();
        }

        return validationResult;
    }

    protected internal abstract ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options);
}