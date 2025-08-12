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

[Keyword("items")]
[JsonConverter(typeof(SingleSchemaJsonConverter<ItemsKeyword>))]
internal class ItemsKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema, IJsonSchemaResourceNodesCleanable
{
    private JsonSchema _schema = null!;

    public JsonSchema Schema
    {
        get => _schema;
        init => _schema = value;
    }

    public PrefixItemsKeyword? PrefixItemsKeyword { get; set; }

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
        private readonly ItemsKeyword _itemsKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
     
        private ValidationResult? _fastReturnResult;

        public Validator(ItemsKeyword itemsKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _itemsKeyword = itemsKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            int idx = 0;
            foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
            {
                if (_itemsKeyword.PrefixItemsKeyword is not null && idx < _itemsKeyword.PrefixItemsKeyword.SubSchemas.Count)
                {
                    idx++;
                    continue;
                }

                ValidationResult validationResult = _itemsKeyword.Schema.Validate(instanceItem, _options);
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
