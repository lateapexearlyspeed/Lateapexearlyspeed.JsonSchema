using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords;

[Keyword("ext-custom-NonGenericObjectValidation")]
[JsonConverter(typeof(ExtendedKeywordJsonConverter))]
internal class ObjectCustomValidationKeyword : KeywordBase
{
    private readonly Type _type;
    private readonly Func<object, bool> _validator;
    private readonly Func<object, string> _errorMessageFunc;

    public ObjectCustomValidationKeyword(Type type, Func<object, bool> validator, Func<object, string> errorMessageFunc)
    {
        _type = type;
        _validator = validator;
        _errorMessageFunc = errorMessageFunc;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        object instanceData;
        try
        {
            instanceData = instance.Deserialize(_type)!;
        }
        catch (Exception)
        {
            return ValidationResult.CreateFailedResult(ResultCode.FailedToDeserialize, $"Failed to deserialize to type: {_type}", options.ValidationPathStack,
                Name, instance.Location);
        }

        return _validator(instanceData)
            ? ValidationResult.ValidResult
            : ValidationResult.CreateFailedResult(ResultCode.FailedForCustomValidation, _errorMessageFunc(instanceData), options.ValidationPathStack,
                Name, instance.Location);
    }
}