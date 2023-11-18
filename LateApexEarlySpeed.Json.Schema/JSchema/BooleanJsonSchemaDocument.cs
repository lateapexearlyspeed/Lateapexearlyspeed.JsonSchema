using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BooleanJsonSchemaDocument : IJsonSchemaDocument
{
    private readonly bool _alwaysValid;

    public static IJsonSchemaDocument True { get; } = new BooleanJsonSchemaDocument(true);

    public static IJsonSchemaDocument False { get; } = new BooleanJsonSchemaDocument(false);

    private BooleanJsonSchemaDocument(bool alwaysValid)
    {
        _alwaysValid = alwaysValid;
    }

    public ValidationResult Validate(JsonInstanceElement instance)
    {
        return _alwaysValid ? ValidationResult.ValidResult : ValidationResult.CreateFailedResult(ResultCode.AlwaysFailedJsonSchema, "Boolean false json schema document occurs", null, null, instance.Location);
    }
}