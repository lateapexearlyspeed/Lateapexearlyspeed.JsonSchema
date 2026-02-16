using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters.Draft7;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

[Keyword("dependencies")]
[JsonConverter(typeof(DependenciesKeywordJsonConverter))]
internal class DependenciesKeyword : KeywordBase, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable
{
    private readonly Dictionary<string, JsonSchema>? _dependenciesSchema;
    private readonly Dictionary<string, string[]>? _dependenciesProperty;

    public DependenciesKeyword(IDictionary<string, JsonSchema>? dependenciesSchema, IDictionary<string, string[]>? dependenciesProperty, bool propertyNameIgnoreCase)
    {
        if (dependenciesSchema is not null)
        {
            _dependenciesSchema = new Dictionary<string, JsonSchema>(dependenciesSchema, propertyNameIgnoreCase ? StringComparer.OrdinalIgnoreCase : null);

            foreach ((string propertyName, JsonSchema schema) in _dependenciesSchema)
            {
                schema.Name = propertyName;
            }
        }

        if (dependenciesProperty is not null)
        {
            _dependenciesProperty = new Dictionary<string, string[]>(dependenciesProperty);
        }
    }

    public IReadOnlyDictionary<string, JsonSchema>? DependenciesSchema => _dependenciesSchema;

    public IReadOnlyDictionary<string, string[]>? DependenciesProperty => _dependenciesProperty;

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
        private readonly List<IValidator> _subValidators = new(2);

        public Validator(DependenciesKeyword keyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            if (keyword._dependenciesSchema is not null)
            {
                _subValidators.Add(new DependentSchemasKeyword.Validator(keyword._dependenciesSchema, instance, options));
            }
            
            if (keyword._dependenciesProperty is not null)
            {
                _subValidators.Add(new DependentRequiredKeyword.Validator(keyword._dependenciesProperty, keyword.Name, instance, options));
            }
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (IValidator subValidator in _subValidators)
            {
                foreach (ValidationResult validationResult in subValidator.EnumerateValidationResults())
                {
                    yield return validationResult;
                }
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            foreach (IValidator subValidator in _subValidators)
            {
                if (subValidator.CanFinishFast(out validationResult))
                {
                    return true;
                }
            }

            validationResult = null;
            return false;
        }

        public ResultTuple Result => _subValidators.Any(validator => !validator.Result.IsValid) ? ResultTuple.Invalid(null) : ResultTuple.Valid();
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return _dependenciesSchema?.GetValueOrDefault(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        // ReSharper disable MergeConditionalExpression
        return _dependenciesSchema is null ? Enumerable.Empty<ISchemaContainerElement>() : _dependenciesSchema.Values;
        // ReSharper restore MergeConditionalExpression
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        if (_dependenciesSchema is null)
        {
            return;
        }

        foreach ((string propertyName, JsonSchema schema) in _dependenciesSchema)
        {
            if (schema is BodyJsonSchema bodyJsonSchema)
            {
                BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _dependenciesSchema[propertyName] = newSchema);
            }
        }
    }
}