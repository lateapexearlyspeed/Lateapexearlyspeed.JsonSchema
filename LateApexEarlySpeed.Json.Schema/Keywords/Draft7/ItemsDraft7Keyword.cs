using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters.Draft7;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

[Keyword("items")]
[Dialect(DialectKind.Draft7, DialectKind.Draft201909)]
[JsonConverter(typeof(ItemsDraft7KeywordJsonConverter))]
internal abstract class ItemsDraft7Keyword : KeywordBase
{
}

[JsonConverter(typeof(SubSchemaCollectionJsonConverter<ItemsWithMultiSchemasKeyword>))]
internal class ItemsWithMultiSchemasKeyword : ItemsDraft7Keyword, ISchemaContainerElement, ISubSchemaCollection, IJsonSchemaResourceNodesCleanable
{
    private readonly JsonSchema[] _schemas = null!;

    public IReadOnlyList<JsonSchema> SubSchemas
    {
        get => _schemas;

        init
        {
            _schemas = value.ToArray();
            for (int i = 0; i < _schemas.Length; i++)
            {
                _schemas[i].Name = i.ToString();
            }
        }
    }

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

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return ((ISubSchemaCollection)this).GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return ((ISubSchemaCollection)this).EnumerateElements();
    }

    public bool IsSchemaType => false;
    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        for (int i = 0; i < _schemas.Length; i++)
        {
            if (_schemas[i] is BodyJsonSchema bodyJsonSchema)
            {
                int localIdx = i;
                BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _schemas[localIdx] = newSchema);
            }
        }
    }
}

[JsonConverter(typeof(SingleSchemaJsonConverter<ItemsWithOneSchemaKeyword>))]
internal class ItemsWithOneSchemaKeyword : ItemsDraft7Keyword, ISchemaContainerElement, ISingleSubSchema, IJsonSchemaResourceNodesCleanable
{
    private JsonSchema _schema = null!;

    public JsonSchema Schema
    {
        get => _schema;
        init => _schema = value;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(Schema, instance, options), options.OutputFormat);
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

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        if (_schema is BodyJsonSchema bodyJsonSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _schema = newSchema);
        }
    }
}

