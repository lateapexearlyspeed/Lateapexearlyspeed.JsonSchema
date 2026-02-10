using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

[Keyword("additionalItems")]
[Dialect(DialectKind.Draft7, DialectKind.Draft201909)]
[JsonConverter(typeof(SingleSchemaJsonConverter<AdditionalItemsKeyword>))]
internal class AdditionalItemsKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema, IJsonSchemaResourceNodesCleanable
{
    private JsonSchema _schema = null!;

    public JsonSchema Schema
    {
        get => _schema;
        init => _schema = value;
    }

    public ItemsWithMultiSchemasKeyword? ItemsWithMultiSchemasKeyword { private get; set; }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (ItemsWithMultiSchemasKeyword is null || instance.ValueKind != JsonValueKind.Array)
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
            Debug.Assert(_keyword.ItemsWithMultiSchemasKeyword is not null);
            int prefixSchemasCount = _keyword.ItemsWithMultiSchemasKeyword.SubSchemas.Count;

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

    public ISchemaContainerElement? GetSubElement(string name) => _schema.GetSubElement(name);

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return _schema;
    }

    public bool IsSchemaType => true;
    public JsonSchema GetSchema() => _schema;
    
    public void RemoveIdFromAllChildrenSchemaElements()
    {
        if (_schema is BodyJsonSchema bodyJsonSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _schema = newSchema);
        }
    }
}