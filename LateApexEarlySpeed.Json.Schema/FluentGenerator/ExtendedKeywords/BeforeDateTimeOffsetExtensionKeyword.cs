using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-BeforeDateTimeOffset")]
internal class BeforeDateTimeOffsetExtensionKeyword : KeywordBase
{
    private readonly string[]? _formats;
    private readonly DateTimeOffset _before;

    public BeforeDateTimeOffsetExtensionKeyword(string[]? formats, DateTimeOffset before)
    {
        _formats = formats;
        _before = before;
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
            return ValidationResult.CreateFailedResult(ResultCode.InvalidFormat, DateTimeOffsetFormatExtensionKeyword.ErrorMessage(), options.ValidationPathStack, Name, instance.Location);
        }

        return data < _before
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.NotBeforeSpecifiedTimePoint, ErrorMessage(data, _before), options.ValidationPathStack, Name, instance.Location);
    }

    private static string ErrorMessage(DateTimeOffset actual, DateTimeOffset expectedBefore)
    {
        return $"Actual time point: {actual} is not before expected time point: {expectedBefore}";
    }
}