using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BooleanJsonSchema : JsonSchema
{
    private readonly bool _alwaysValid;

    protected BooleanJsonSchema(bool alwaysValid)
    {
        _alwaysValid = alwaysValid;
    }

    public static BooleanJsonSchema True => new(true);

    public static BooleanJsonSchema False => new(false);

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        return _alwaysValid 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.AlwaysFailedJsonSchema, "Boolean false json schema occurs", options.ValidationPathStack, null);
    }

    public override ISchemaContainerElement? GetSubElement(string name)
    {
        return null;
    }

    public override IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return Enumerable.Empty<ISchemaContainerElement>();
    }
}