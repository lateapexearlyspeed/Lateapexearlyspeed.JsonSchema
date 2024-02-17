using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-EqualsDateTime")]
internal class EqualsDateTimeExtensionKeyword : KeywordBase
{
    private readonly string[]? _formats;
    private readonly DateTime _value;

    public EqualsDateTimeExtensionKeyword(string[]? formats, DateTime value)
    {
        _formats = formats;
        _value = value;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        bool canParse = _formats is null
            ? DateTime.TryParse(instance.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data)
            : DateTime.TryParseExact(instance.GetString()!, _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out data);

        if (!canParse)
        {
            return ValidationResult.CreateFailedResult(ResultCode.InvalidFormat, DateTimeFormatExtensionKeyword.ErrorMessage(), options.ValidationPathStack, Name, instance.Location);
        }

        return data == _value
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.UnexpectedValue, ErrorMessage(data, _value), options.ValidationPathStack, Name, instance.Location);
    }

    private static string ErrorMessage(DateTime actual, DateTime expectedValue)
    {
        return $"Actual time point: {actual} not equal to expected time point: {expectedValue}";
    }
}