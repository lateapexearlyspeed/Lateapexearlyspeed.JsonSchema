using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BooleanJsonSchema : JsonSchema
{
    public bool AlwaysValid { get; }

    protected BooleanJsonSchema(bool alwaysValid)
    {
        AlwaysValid = alwaysValid;
    }

    public static BooleanJsonSchema True => new(true);

    public static BooleanJsonSchema False => new(false);

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return AlwaysValid 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.AlwaysFailedJsonSchema, "Boolean false json schema occurs", options.ValidationPathStack, null, instance.Location);
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