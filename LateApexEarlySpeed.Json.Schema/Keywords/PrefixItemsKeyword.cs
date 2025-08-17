using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("prefixItems")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<PrefixItemsKeyword>))]
internal class PrefixItemsKeyword : KeywordBase, ISchemaContainerElement, ISubSchemaCollection, IJsonSchemaResourceNodesCleanable
{
    private readonly JsonSchema[] _subSchemas = null!;

    public PrefixItemsKeyword()
    {
    }

    public PrefixItemsKeyword(IEnumerable<JsonSchema> subSchemas)
    {
        _subSchemas = CreateSubSchema(subSchemas);
    }

    public IReadOnlyList<JsonSchema> SubSchemas
    {
        get => _subSchemas;

        init => _subSchemas = CreateSubSchema(value);
    }

    [Pure]
    private static JsonSchema[] CreateSubSchema(IEnumerable<JsonSchema> subSchemas)
    {
        JsonSchema[] result = subSchemas.ToArray();

        for (int i = 0; i < result.Length; i++)
        {
            result[i].Name = i.ToString();
        }

        return result;
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
        private readonly PrefixItemsKeyword _prefixItemsKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(PrefixItemsKeyword prefixItemsKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _prefixItemsKeyword = prefixItemsKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            int schemaIdx = 0;
            foreach (JsonInstanceElement instanceItem in _instance.EnumerateArray())
            {
                if (schemaIdx >= _prefixItemsKeyword.SubSchemas.Count)
                {
                    break;
                }

                ValidationResult validationResult = _prefixItemsKeyword.SubSchemas[schemaIdx].Validate(instanceItem, _options);
                if (!validationResult.IsValid)
                {
                    _fastReturnResult = validationResult;
                }

                yield return validationResult;

                schemaIdx++;
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
        for (int i = 0; i < _subSchemas.Length; i++)
        {
            if (_subSchemas[i] is BodyJsonSchema bodyJsonSchema)
            {
                int localIdx = i;
                BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _subSchemas[localIdx] = newSchema);
            }
        }
    }
}
