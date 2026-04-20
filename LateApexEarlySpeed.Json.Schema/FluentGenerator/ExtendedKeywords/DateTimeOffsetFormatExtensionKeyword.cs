using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-DateTimeOffsetFormat")]
internal class DateTimeOffsetFormatExtensionKeyword : KeywordBase
{
    private readonly string[]? _formats;

    public DateTimeOffsetFormatExtensionKeyword(string[]? formats = null)
    {
        _formats = formats;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.String)
        {
            return ValidationResult.ValidResult;
        }

        bool canParse = _formats is null
            ? DateTimeOffset.TryParse(instance.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            : DateTimeOffset.TryParseExact(instance.GetString()!, _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

        return canParse
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.InvalidFormat, ErrorMessage(), options.ValidationPathStack, Name, instance.Location));
    }

    internal static string ErrorMessage()
    {
        return $"Invalid string format for '{nameof(DateTimeOffset)}' type";
    }
}