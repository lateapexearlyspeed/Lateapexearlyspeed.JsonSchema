using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("dependentRequired")]
[JsonConverter(typeof(DependentRequiredKeywordJsonConverter))]
internal class DependentRequiredKeyword : KeywordBase
{
    public Dictionary<string, string[]> DependentProperties { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(this, instance, options), options.OutputFormat);
    }

    public static string ErrorMessage(string dependentProperty, string requiredProp)
    {
        return $"Instance contains property: '{dependentProperty}' but not contains dependent property: '{requiredProp}'";
    }

    private class Validator : IValidator
    {
        private readonly DependentRequiredKeyword _dependentRequiredKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(DependentRequiredKeyword dependentRequiredKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _dependentRequiredKeyword = dependentRequiredKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            var instancePropertyNames = new HashSet<string>(_instance.EnumerateObject().Select(p => p.Name));

            foreach (KeyValuePair<string, string[]> dependentProperty in _dependentRequiredKeyword.DependentProperties)
            {
                if (instancePropertyNames.Contains(dependentProperty.Key))
                {
                    foreach (string requiredProp in dependentProperty.Value)
                    {
                        if (!instancePropertyNames.Contains(requiredProp))
                        {
                            _fastReturnResult = ValidationResult.SingleErrorFailedResult(new ValidationError(
                                ResultCode.NotFoundRequiredDependentProperty,
                                ErrorMessage(dependentProperty.Key, requiredProp),
                                _options.ValidationPathStack,
                                _dependentRequiredKeyword.Name,
                                _instance.Location));

                            yield return _fastReturnResult;
                        }
                    }
                }
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            return (validationResult = _fastReturnResult) is not null;
        }

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }
}
