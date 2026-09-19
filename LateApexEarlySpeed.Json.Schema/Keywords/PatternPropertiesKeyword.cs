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
internal class PatternPropertiesKeyword : ValidationKeywordBase, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable, IPropertyEvaluator
{
    private readonly Dictionary<string, JsonSchema> _patternSchemas;

    public PatternPropertiesKeyword(Dictionary<string, JsonSchema> patternSchemas)
    {
        _patternSchemas = new Dictionary<string, JsonSchema>(patternSchemas);
    }

    public IReadOnlyDictionary<string, JsonSchema> PatternSchemas => _patternSchemas;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        var validator = new Validator(this, instance, options);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    private struct Validator : IValidator
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

        public void CollectValidationResults(ref ValidationCompositionContext context)
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

                        if (!context.ReportValidationResult(validationResult, _fastReturnResult))
                        {
                            return;
                        }
                    }
                }
            }
        }

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
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

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        foreach ((string pattern, JsonSchema schema) in _patternSchemas)
        {
            if (schema is BodyJsonSchema bodyJsonSchema)
            {
                BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _patternSchemas[pattern] = newSchema);
            }
        }
    }

    private readonly struct PropertyEvaluator : IPropertyCoreEvaluator
    {
        private readonly PatternPropertiesKeyword _patternPropertiesKeyword;
        private readonly JsonInstanceElement _instance;

        public PropertyEvaluator(PatternPropertiesKeyword patternPropertiesKeyword, JsonInstanceElement instance)
        {
            _patternPropertiesKeyword = patternPropertiesKeyword;
            _instance = instance;
        }

        public string Name => _patternPropertiesKeyword.Name;

        public IReadOnlyCollection<string> GetEvaluatedProperties(JsonSchemaOptions options)
        {
            if (_instance.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<string>();
            }

            List<string>? evaluatedProperties = null;

            foreach (JsonInstanceProperty jsonProperty in _instance.EnumerateObject())
            {
                string propertyName = jsonProperty.Name;

                foreach (KeyValuePair<string, JsonSchema> patternSchema in _patternPropertiesKeyword._patternSchemas)
                {
                    if (RegexMatcher.IsMatch(patternSchema.Key, propertyName, options.RegexMatchTimeout))
                    {
                        evaluatedProperties ??= new List<string>();
                        evaluatedProperties.Add(propertyName);

                        break;
                    }
                }
            }

            return evaluatedProperties ?? (IReadOnlyCollection<string>)Array.Empty<string>();
        }
    }

    public IReadOnlyCollection<string> GetEvaluatedProperties(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var propertyEvaluator = new PropertyEvaluator(this, instance);
        return propertyEvaluator.GetEvaluatedPropertiesWithLocation(options);
    }
}
