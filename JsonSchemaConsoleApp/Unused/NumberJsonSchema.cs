using System.Text.Json;
using JsonSchemaConsoleApp.Unused;

namespace JsonSchemaConsoleApp;

internal class NumberJsonSchema : BodyJsonSchema
{
    public const string TypeName = "number";

    private readonly JsonElement _schema;

    public NumberJsonSchema(JsonElement schema)
    {
        _schema = schema;
    }

    public bool Validate(JsonElement jsonInstance)
    {
        if (jsonInstance.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        double doubleInstance = jsonInstance.GetDouble();

        if (!RangeValidator.Validate(_schema, doubleInstance))
        {
            return false;
        }

        return true;
    }
}