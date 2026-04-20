using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-AfterDateTimeOffset")]
internal class AfterDateTimeOffsetExtensionKeyword : KeywordBase
{
    private readonly string[]? _formats;
    private readonly DateTimeOffset _after;

    public AfterDateTimeOffsetExtensionKeyword(string[]? formats, DateTimeOffset after)
    {
        _formats = formats;
        _after = after;
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

        return data > _after
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.NotAfterSpecifiedTimePoint, ErrorMessage(data, _after), options.ValidationPathStack, Name, instance.Location));
    }

    private static string ErrorMessage(DateTimeOffset actual, DateTimeOffset expectedAfter)
    {
        return $"Actual time point: {actual} is not after expected time point: {expectedAfter}";
    }
}