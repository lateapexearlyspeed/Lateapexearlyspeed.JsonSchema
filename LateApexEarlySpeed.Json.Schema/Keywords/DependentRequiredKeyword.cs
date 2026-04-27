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

        var validator = new Validator(DependentProperties, Name, instance, options);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    public static string ErrorMessage(string dependentProperty, string requiredProp)
    {
        return $"Instance contains property '{dependentProperty}' but does not contain dependent property '{requiredProp}'";
    }

    internal struct Validator : IValidator
    {
        private readonly IReadOnlyDictionary<string, string[]> _dependentProperties;
        private readonly string _keywordName;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(IReadOnlyDictionary<string, string[]> dependentProperties, string keywordName, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _dependentProperties = dependentProperties;
            _keywordName = keywordName;
            _instance = instance;
            _options = options;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            var instancePropertyNames = new HashSet<string>(_instance.EnumerateObject().Select(p => p.Name));

            foreach (KeyValuePair<string, string[]> dependentProperty in _dependentProperties)
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
                                _keywordName,
                                _instance.Location));

                            if (!context.Report(_fastReturnResult, _fastReturnResult))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        public readonly ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }
}
