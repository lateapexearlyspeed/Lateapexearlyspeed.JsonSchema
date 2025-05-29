using System.Globalization;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-DateTime")]
internal class DateTimeCustomValidationKeyword : KeywordBase
{
    private readonly string[]? _formats;
    private readonly Func<DateTime, bool> _validator;
    private readonly Func<DateTime, string> _errorMessageFunc;

    public DateTimeCustomValidationKeyword(string[]? formats, Func<DateTime, bool> validator, Func<DateTime, string> errorMessageFunc)
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
            ? DateTime.TryParse(instance.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data)
            : DateTime.TryParseExact(instance.GetString()!, _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out data);

        if (!canParse)
        {
            return ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.InvalidFormat, DateTimeFormatExtensionKeyword.ErrorMessage(), options.ValidationPathStack, Name, instance.Location));
        }

        return _validator(data)
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.FailedForCustomValidation, _errorMessageFunc(data), options.ValidationPathStack,
                Name, instance.Location));
    }
}