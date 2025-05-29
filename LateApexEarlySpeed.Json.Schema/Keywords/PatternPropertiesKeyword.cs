using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("patternProperties")]
[JsonConverter(typeof(PatternPropertiesKeywordJsonConverter))]
internal class PatternPropertiesKeyword : KeywordBase, ISchemaContainerElement
{
    public PatternPropertiesKeyword(Dictionary<string, JsonSchema> patternSchemas)
    {
        PatternSchemas = new Dictionary<string, JsonSchema>(patternSchemas);
    }

    public IReadOnlyDictionary<string, JsonSchema> PatternSchemas { get; }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(this, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
    {
        private readonly PatternPropertiesKeyword _patternPropertiesKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(PatternPropertiesKeyword patternPropertiesKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _patternPropertiesKeyword = patternPropertiesKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonInstanceProperty jsonProperty in _instance.EnumerateObject())
            {
                string propertyName = jsonProperty.Name;
                JsonInstanceElement propertyValue = jsonProperty.Value;

                foreach (KeyValuePair<string, JsonSchema> patternSchema in _patternPropertiesKeyword.PatternSchemas)
                {
                    if (RegexMatcher.IsMatch(patternSchema.Key, propertyName, _options.RegexMatchTimeout))
                    {
                        ValidationResult validationResult = patternSchema.Value.Validate(propertyValue, _options);
                        if (!validationResult.IsValid)
                        {
                            _fastReturnResult = validationResult;
                        }

                        yield return validationResult;
                    }
                }
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            validationResult = _fastReturnResult;

            return _fastReturnResult is not null;
        }

        public ResultTuple Result
        {
            get
            {
                if (_fastReturnResult is null)
                {
                    return ResultTuple.Valid();
                }

                var curError = new ValidationError(ResultCode.FailedInSubSchema, ValidationError.ErrorMessageForFailedInSubSchema, _options.ValidationPathStack, _patternPropertiesKeyword.Name, _instance.Location);
                return ResultTuple.WithError(curError);
            }
        }
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return PatternSchemas.TryGetValue(name, out JsonSchema schema) 
            ? schema
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return PatternSchemas.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public bool ContainsMatchedPattern(string propertyName, TimeSpan matchTimeout)
    {
        return PatternSchemas.Any(regexAndSchema => RegexMatcher.IsMatch(regexAndSchema.Key, propertyName, matchTimeout));
    }
}
