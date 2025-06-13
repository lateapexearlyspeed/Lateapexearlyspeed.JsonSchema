using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("additionalProperties")]
[JsonConverter(typeof(SingleSchemaJsonConverter<AdditionalPropertiesKeyword>))]
internal class AdditionalPropertiesKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema
{
    public JsonSchema Schema { get; init; } = null!;

    public PropertiesKeyword? PropertiesKeyword { get; set; }

    public PatternPropertiesKeyword? PatternPropertiesKeyword { get; set; }

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
        private readonly AdditionalPropertiesKeyword _additionalPropertiesKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        
        private ValidationResult? _fastReturnResult;

        public Validator(AdditionalPropertiesKeyword additionalPropertiesKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _additionalPropertiesKeyword = additionalPropertiesKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonInstanceProperty jsonProperty in _instance.EnumerateObject())
            {
                string propertyName = jsonProperty.Name;

                bool containsInPropertiesKeyword = _additionalPropertiesKeyword.PropertiesKeyword is not null && _additionalPropertiesKeyword.PropertiesKeyword.ContainsPropertyName(propertyName);

                if (containsInPropertiesKeyword)
                {
                    continue;
                }

                bool containsMatchedPattern = _additionalPropertiesKeyword.PatternPropertiesKeyword is not null && _additionalPropertiesKeyword.PatternPropertiesKeyword.ContainsMatchedPattern(propertyName, _options.RegexMatchTimeout);

                if (!containsMatchedPattern)
                {
                    ValidationResult validationResult = _additionalPropertiesKeyword.Schema.Validate(jsonProperty.Value, _options);
                    if (!validationResult.IsValid)
                    {
                        _fastReturnResult = validationResult;
                    }

                    yield return validationResult;
                }
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            return (validationResult = _fastReturnResult) is not null;
        }

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return Schema.GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return Schema;
    }

    public bool IsSchemaType => true;


    public JsonSchema GetSchema()
    {
        return Schema;
    }
}
