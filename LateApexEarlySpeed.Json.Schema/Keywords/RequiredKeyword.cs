using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("required")]
[JsonConverter(typeof(RequiredKeywordJsonConverter))]
internal class RequiredKeyword : KeywordBase
{
    private readonly bool _propertyNameIgnoreCase;
    private readonly string[] _requiredProperties;

    public IReadOnlyList<string> RequiredProperties => _requiredProperties;

    public RequiredKeyword(IEnumerable<string> requiredProperties, bool propertyNameIgnoreCase)
    {
        _propertyNameIgnoreCase = propertyNameIgnoreCase;
        _requiredProperties = requiredProperties.ToArray();
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        if (_requiredProperties.Length == 0)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(this, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
    {
        private readonly RequiredKeyword _requiredKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(RequiredKeyword requiredKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _requiredKeyword = requiredKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            HashSet<string> instanceProperties = _instance.EnumerateObject().Select(prop => prop.Name)
                .ToHashSet(_requiredKeyword._propertyNameIgnoreCase ? StringComparer.OrdinalIgnoreCase : null);

            foreach (string requiredProperty in _requiredKeyword._requiredProperties)
            {
                ValidationResult validationResult;

                if (instanceProperties.Contains(requiredProperty))
                {
                    validationResult = ValidationResult.ValidResult;
                }
                else
                {
                    var curError = new ValidationError(ResultCode.NotFoundRequiredProperty, ErrorMessage(requiredProperty), _options.ValidationPathStack, _requiredKeyword.Name, _instance.Location);
                    validationResult = ValidationResult.SingleErrorFailedResult(curError);

                    _fastReturnResult = validationResult;
                }

                yield return validationResult;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            return (validationResult = _fastReturnResult) is not null;
        }

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }

    public static string ErrorMessage(string missedPropertyName)
    {
        return $"Instance not contain required property '{missedPropertyName}'";
    }
}
