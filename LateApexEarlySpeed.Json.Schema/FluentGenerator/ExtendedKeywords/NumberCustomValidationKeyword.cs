using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

public abstract class NumberCustomValidationKeyword<T> : KeywordBase
{
    private readonly Func<T, bool> _validator;
    private readonly Func<T, string> _errorMessageFunc;

    protected NumberCustomValidationKeyword(Func<T, bool> validator, Func<T, string> errorMessageFunc)
    {
        _validator = validator;
        _errorMessageFunc = errorMessageFunc;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Number)
        {
            return ValidationResult.ValidResult;
        }

        if (!TryGetNumber(instance, out T instanceData))
        {
            return ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.FailedForCustomValidation, ErrorMessageForTypeConvert(instance.ToString()), options.ValidationPathStack, Name, instance.Location));
        }

        return _validator(instanceData)
            ? ValidationResult.ValidResult
            : ValidationResult.SingleErrorFailedResult(new ValidationError(ResultCode.FailedForCustomValidation, _errorMessageFunc(instanceData), options.ValidationPathStack,
                Name, instance.Location));
    }

    protected abstract bool TryGetNumber(JsonInstanceElement instance, out T value);

    protected internal static string ErrorMessageForTypeConvert(string instanceJson) => $"Cannot convert json value:{instanceJson} to specified type:{typeof(T)}";
}