using System.Text.Json;
using System.Text.RegularExpressions;

namespace JsonSchemaConsoleApp;

internal class StringJsonSchema : BodyJsonSchema
{
    public const string TypeName = "string";
        
    private const string MinLengthKeyword = "minLength";
    private const string MaxLengthKeyword = "maxLength";
    private const string PatternKeyword = "pattern";
    private const string FormatKeyword = "format";

    private readonly JsonElement _schema;
    private readonly bool _shouldValidateFormat;


    public StringJsonSchema(JsonElement schema, bool shouldValidateFormat)
    {
        _schema = schema;
        _shouldValidateFormat = shouldValidateFormat;
    }

    public bool Validate(JsonElement jsonInstance)
    {
        if (jsonInstance.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        return Validate(jsonInstance.GetString()!);
    }

    public bool Validate(string stringInstance)
    {
        if (_schema.TryGetKeyword(MinLengthKeyword, out uint minLen))
        {
            if (stringInstance.Length < minLen)
            {
                return false;
            }
        }

        if (_schema.TryGetKeyword(MaxLengthKeyword, out uint maxLen))
        {
            if (stringInstance.Length > maxLen)
            {
                return false;
            }
        }

        if (_schema.TryGetKeyword(PatternKeyword, out string? pattern))
        {
            if (!Regex.IsMatch(stringInstance, pattern))
            {
                return false;
            }
        }

        if (_shouldValidateFormat && _schema.TryGetKeyword(FormatKeyword, out string? format))
        {
            if (!StringFormatParser.Create(format).IsInstanceValid(stringInstance))
            {
                return false;
            }
        }

        return true;
    }
}