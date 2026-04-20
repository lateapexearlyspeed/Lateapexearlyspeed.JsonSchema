using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-EqualsDateTimeOffset")]
internal class EqualsDateTimeOffsetExtensionKeyword : KeywordBase
{
    private readonly string[]? _formats;
    private readonly DateTimeOffset _value;

    public EqualsDateTimeOffsetExtensionKeyword(string[]? formats, DateTimeOffset value)
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
            ? DateTimeOffset.TryParse(instance.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset data)
            : DateTimeOffset.TryParseExact(instance.GetString()!, _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out data);

        if (!canParse)
        {
            return ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.InvalidFormat, DateTimeOffsetFormatExtensionKeyword.ErrorMessage(), options.ValidationPathStack, Name, instance.Location));
        }

        return data == _value
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.UnexpectedValue, ErrorMessage(data, _value), options.ValidationPathStack, Name, instance.Location));
    }

    private static string ErrorMessage(DateTimeOffset actual, DateTimeOffset expectedValue)
    {
        return $"Actual time point: {actual} not equal to expected time point: {expectedValue}";
    }
}