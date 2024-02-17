using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-ObjectValidation")]
internal class CustomValidationKeyword<T> : KeywordBase
{
    private readonly Func<T?, bool> _validator;
    private readonly Func<T?, string> _errorMessageFunc;

    public CustomValidationKeyword(Func<T?, bool> validator, Func<T?, string> errorMessageFunc)
    {
        _validator = validator;
        _errorMessageFunc = errorMessageFunc;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        T? instanceData;
        try
        {
            instanceData = instance.Deserialize<T>();
        }
        catch (Exception)
        {
            return ValidationResult.CreateFailedResult(ResultCode.FailedToDeserialize, $"Failed to deserialize to type: {typeof(T)}", options.ValidationPathStack,
                Name, instance.Location);
        }

        return _validator(instanceData)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.FailedForCustomValidation, _errorMessageFunc(instanceData), options.ValidationPathStack,
                Name, instance.Location);
    }
}