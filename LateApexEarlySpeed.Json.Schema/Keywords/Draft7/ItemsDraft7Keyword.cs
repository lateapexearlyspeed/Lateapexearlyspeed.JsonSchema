using System.Diagnostics.CodeAnalysis;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

internal class DependenciesKeyword : KeywordBase
{
    private readonly IReadOnlyDictionary<string, JsonSchema>? _dependenciesSchema;
    private readonly IReadOnlyDictionary<string, string[]>? _dependenciesProperty;

    public DependenciesKeyword(IReadOnlyDictionary<string, JsonSchema>? dependenciesSchema, IReadOnlyDictionary<string, string[]>? dependenciesProperty)
    {
        _dependenciesSchema = dependenciesSchema;
        _dependenciesProperty = dependenciesProperty;
    }

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
}

internal class AdditionalItemsKeyword : KeywordBase
{
    private readonly JsonSchema _schema;
    private readonly ItemsWithMultiSchemasKeyword _itemsWithMultiSchemasKeyword;

    public AdditionalItemsKeyword(JsonSchema schema, ItemsWithMultiSchemasKeyword itemsWithMultiSchemasKeyword)
    {
        _schema = schema;
        _itemsWithMultiSchemasKeyword = itemsWithMultiSchemasKeyword;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(this, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
    {
        private readonly AdditionalItemsKeyword _keyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        
        private ValidationResult? _fastReturnResult;

        public Validator(AdditionalItemsKeyword keyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _keyword = keyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            int prefixSchemasCount = _keyword._itemsWithMultiSchemasKeyword.Schemas.Count;

            int idx = 0;

            foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
            {
                if (idx < prefixSchemasCount)
                {
                    idx++;
                    continue;
                }

                ValidationResult validationResult = _keyword._schema.Validate(instanceItem, _options);

                if (!validationResult.IsValid)
                {
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
}

internal abstract class ItemsDraft7Keyword : KeywordBase
{
}

internal class ItemsWithMultiSchemasKeyword : ItemsDraft7Keyword
{
    private readonly JsonSchema[] _schemas;

    public ItemsWithMultiSchemasKeyword(JsonSchema[] schemas)
    {
        _schemas = schemas;
    }

    public IReadOnlyCollection<JsonSchema> Schemas => _schemas;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(_schemas, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
    {
        private readonly JsonSchema[] _schemas;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(JsonSchema[] schemas, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _schemas = schemas;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            int idx = 0;

            foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
            {
                if (idx >= _schemas.Length)
                {
                    break;
                }

                ValidationResult validationResult = _schemas[idx++].Validate(instanceItem, _options);

                if (!validationResult.IsValid)
                {
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
}

internal class ItemsWithOneSchemaKeyword : ItemsDraft7Keyword
{
    private readonly JsonSchema _schema;

    public ItemsWithOneSchemaKeyword(JsonSchema schema)
    {
        _schema = schema;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(_schema, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
    {
        private readonly JsonSchema _schema;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(JsonSchema schema, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _schema = schema;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonInstanceElement instanceElement in _instance.EnumerateArray())
            {
                ValidationResult validationResult = _schema.Validate(instanceElement, _options);

                if (!validationResult.IsValid)
                {
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
}

