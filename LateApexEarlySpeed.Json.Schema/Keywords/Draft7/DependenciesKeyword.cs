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

        var validator = new Validator(this, instance, options);
        return ValidationResultsComposer.ComposeV2(ref validator, options.OutputFormat);
    }

    private struct Validator : IValidator
    {
        private DependentSchemasKeyword.Validator? _dependentSchemaValidator;
        private DependentRequiredKeyword.Validator? _dependentRequiredValidator;

        public Validator(DependenciesKeyword keyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            if (keyword._dependenciesSchema is not null)
            {
                _dependentSchemaValidator = new DependentSchemasKeyword.Validator(keyword._dependenciesSchema, instance, options);
            }
            
            if (keyword._dependenciesProperty is not null)
            {
                _dependentRequiredValidator = new DependentRequiredKeyword.Validator(keyword._dependenciesProperty, keyword.Name, instance, options);
            }
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            if (_dependentSchemaValidator.HasValue)
            {
                foreach (ValidationResult result in _dependentSchemaValidator.Value.EnumerateValidationResults())
                {
                    yield return result;
                }
            }

            if (_dependentRequiredValidator.HasValue)
            {
                foreach (ValidationResult result in _dependentRequiredValidator.Value.EnumerateValidationResults())
                {
                    yield return result;
                }
            }
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            if (_dependentSchemaValidator.HasValue)
            {
                DependentSchemasKeyword.Validator dependentSchemaValidator = _dependentSchemaValidator.Value;
                dependentSchemaValidator.CollectValidationResults(ref context);

                // We need to assign back the validator to the field because of the possibility of updated validator (e.g. fast return result).
                // In that case, the state of the validator is changed, and we need to keep that change for the next calls (like CanFinishFast or Result).
                _dependentSchemaValidator = dependentSchemaValidator;
            }

            if (_dependentRequiredValidator.HasValue)
            {
                DependentRequiredKeyword.Validator dependentRequiredValidator = _dependentRequiredValidator.Value;
                dependentRequiredValidator.CollectValidationResults(ref context);

                // We need to assign back the validator to the field because of the possibility of updated validator (e.g. fast return result).
                // In that case, the state of the validator is changed, and we need to keep that change for the next calls (like CanFinishFast or Result).
                _dependentRequiredValidator = dependentRequiredValidator;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            if (_dependentSchemaValidator.HasValue && _dependentSchemaValidator.Value.CanFinishFast(out validationResult))
            {
                return true;
            }

            if (_dependentRequiredValidator.HasValue && _dependentRequiredValidator.Value.CanFinishFast(out validationResult))
            {
                return true;
            }

            validationResult = null;
            return false;
        }

        public ResultTuple Result
        {
            get
            {
                if (_dependentSchemaValidator.HasValue && !_dependentSchemaValidator.Value.Result.IsValid)
                {
                    return ResultTuple.Invalid(null);
                }

                if (_dependentRequiredValidator.HasValue && !_dependentRequiredValidator.Value.Result.IsValid)
                {
                    return ResultTuple.Invalid(null);
                }

                return ResultTuple.Valid();
            }
        }
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