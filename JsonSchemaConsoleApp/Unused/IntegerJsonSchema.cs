using System.Text.Json;
using JsonSchemaConsoleApp.Unused;

namespace JsonSchemaConsoleApp;

internal class IntegerJsonSchema : BodyJsonSchema
{
    public const string TypeName = "integer";
        
    private const string MultipleOfKeyword = "multipleOf";
    private readonly JsonElement _schema;

    public IntegerJsonSchema(JsonElement schema)
    {
        _schema = schema;
    }

    public bool Validate(JsonElement jsonInstance)
    {
        if (jsonInstance.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        string rawText = jsonInstance.GetRawText();
        int dotIdx = rawText.IndexOf('.');
        if (dotIdx != -1)
        {
            ReadOnlySpan<char> fraction = rawText.AsSpan(dotIdx + 1);
            foreach (char c in fraction)
            {
                if (c != '0')
                {
                    return false;
                }
            }
        }

        int integerInstance = jsonInstance.GetInt32();

        if (_schema.TryGetKeyword(MultipleOfKeyword, out uint multipleOf))
        {
            if (integerInstance % multipleOf != 0)
            {
                return false;
            }
        }

        if (!RangeValidator.Validate(_schema, integerInstance))
        {
            return false;
        }

        return true;
    }
}