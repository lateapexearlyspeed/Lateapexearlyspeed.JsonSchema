using System.Text.Json;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp;

public abstract class ValidationNode
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