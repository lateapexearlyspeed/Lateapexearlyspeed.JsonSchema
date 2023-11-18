using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Common;

public abstract class NamedValidationNode : IValidationNode
{
    public string? Name { get; set; }

    public virtual ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
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

    protected internal abstract ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options);
}