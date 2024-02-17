using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-DateTimeOffset")]
internal class DateTimeOffsetCustomValidationKeyword : KeywordBase
{
    private readonly string[]? _formats;
    private readonly Func<DateTimeOffset, bool> _validator;
    private readonly Func<DateTimeOffset, string> _errorMessageFunc;

    public DateTimeOffsetCustomValidationKeyword(string[]? formats, Func<DateTimeOffset, bool> validator, Func<DateTimeOffset, string> errorMessageFunc)
    {
        _formats = formats;
        _validator = validator;
        _errorMessageFunc = errorMessageFunc;
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

        return _validator(data)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.FailedForCustomValidation, _errorMessageFunc(data), options.ValidationPathStack,
                Name, instance.Location);
    }
}