using System.Text.Json;

namespace JsonSchemaConsoleApp;

internal class BooleanJsonSchemaDocument : IJsonSchemaDocument
{
    private readonly bool _alwaysValid;

    public static IJsonSchemaDocument True { get; } = new BooleanJsonSchemaDocument(true);
    
    public static IJsonSchemaDocument False { get; } = new BooleanJsonSchemaDocument(false);

    private BooleanJsonSchemaDocument(bool alwaysValid)
    {
        _alwaysValid = alwaysValid;
    }

    public ValidationResult Validate(JsonElement instance)
    {
        return _alwaysValid ? ValidationResult.ValidResult : ValidationResult.CreateFailedResult(ResultCode.AlwaysFailed, null);
    }
}