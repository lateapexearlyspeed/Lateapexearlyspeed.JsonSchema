using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("anyOf")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<AnyOfKeyword>))]
internal class AnyOfKeyword : ValidationKeywordBase, ISubSchemaCollection, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable, IPropertyEvaluator, IItemEvaluator
{
    private readonly JsonSchema[] _subSchemas = null!;

    public AnyOfKeyword()
    {
    }

    public AnyOfKeyword(IEnumerable<JsonSchema> subSchemas)
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
        var validator = new Validator(this, instance, options);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    private struct Validator : IValidator
    {
        private readonly AnyOfKeyword _anyOfKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        
        private ValidationResult? _fastReturnResult;

        public Validator(AnyOfKeyword anyOfKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _anyOfKeyword = anyOfKeyword;
            _instance = instance;
            _options = options;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            if (_options.InUnevaluatedPropertiesScope || _options.InUnevaluatedItemsScope)
            {
                byte[] subSchemasStatus = BitmapHelper.CreateBitmap(_anyOfKeyword._subSchemas.Length);

                for (var i = 0; i < _anyOfKeyword._subSchemas.Length; i++)
                {
                    JsonSchema subSchema = _anyOfKeyword._subSchemas[i];
                    ValidationResult result = subSchema.Validate(_instance, _options);
                    if (result.IsValid)
                    {
                        _fastReturnResult = result;
                        BitmapHelper.SetBit(subSchemasStatus, i);
                    }

                    bool reportValidationResult = context.ReportValidationResult(result, null);
                    Debug.Assert(reportValidationResult);
                }

                _options.SetSubSchemaStatusBitmap(_anyOfKeyword, _instance.Location, subSchemasStatus);
            }
            else
            {
                foreach (JsonSchema subSchema in _anyOfKeyword._subSchemas)
                {
                    ValidationResult result = subSchema.Validate(_instance, _options);
                    if (result.IsValid)
                    {
                        _fastReturnResult = result;
                    }

                    if (!context.ReportValidationResult(result, _fastReturnResult))
                    {
                        break;
                    }
                }    
            }
        }

        public ResultTuple Result
        {
            get
            {
                if (_fastReturnResult is null)
                {
                    var curError = new ValidationError(ResultCode.AllSubSchemaFailed, _options.GenerateErrorMessages ? ErrorMessage() : string.Empty, _options.ValidationPathStack, _anyOfKeyword.Name, _instance.Location);

                    return ResultTuple.Invalid(curError);
                }

                return ResultTuple.Valid();
            }
        }
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

    public static string ErrorMessage() => "Instance failed validation against all sub-schemas";

    private readonly struct PropertyEvaluator : IPropertyCoreEvaluator
    {
        private readonly AnyOfKeyword _anyOfKeyword;
        private readonly JsonInstanceElement _instance;

        public PropertyEvaluator(AnyOfKeyword anyOfKeyword, JsonInstanceElement instance)
        {
            _anyOfKeyword = anyOfKeyword;
            _instance = instance;
        }

        public string Name => _anyOfKeyword.Name;

        public IReadOnlyCollection<string> GetEvaluatedProperties(JsonSchemaOptions options)
        {
            return _anyOfKeyword.GetEvaluatedInfoInternal<IPropertyEvaluator, string>(_instance, options);
        }
    }

    IReadOnlyCollection<string> IInfoEvaluator<string>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var propertyEvaluator = new PropertyEvaluator(this, instance);
        return propertyEvaluator.GetEvaluatedPropertiesWithLocation(options);
    }

    IReadOnlyCollection<int> IInfoEvaluator<int>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var itemEvaluator = new ItemEvaluator(this, instance);
        return itemEvaluator.GetEvaluatedItemsWithLocation(options);
    }

    private readonly struct ItemEvaluator : IItemCoreEvaluator
    {
        private readonly AnyOfKeyword _anyOfKeyword;
        private readonly JsonInstanceElement _instance;

        public ItemEvaluator(AnyOfKeyword anyOfKeyword, JsonInstanceElement instance)
        {
            _anyOfKeyword = anyOfKeyword;
            _instance = instance;
        }

        public string Name => _anyOfKeyword.Name;

        public IReadOnlyCollection<int> GetEvaluatedItems(JsonSchemaOptions options)
        {
            return _anyOfKeyword.GetEvaluatedInfoInternal<IItemEvaluator, int>(_instance, options);
        }
    }

    private IReadOnlyCollection<TValue> GetEvaluatedInfoInternal<TEvaluator, TValue>(JsonInstanceElement instance, JsonSchemaOptions options) where TEvaluator : IInfoEvaluator<TValue>
    {
        HashSet<TValue>? result = null;

        byte[] subSchemasValidationStatus = options.GetSubSchemaStatusBitmap(this, instance.Location);

        for (var i = 0; i < _subSchemas.Length; i++)
        {
            JsonSchema subSchema = _subSchemas[i];
            if (!BitmapHelper.IsBitSet(subSchemasValidationStatus, i) || subSchema is not TEvaluator itemEvaluator)
            {
                continue;
            }

            IReadOnlyCollection<TValue> evaluatedItems = itemEvaluator.GetEvaluatedInfos(instance, options);
            if (evaluatedItems.Count != 0)
            {
                result ??= new HashSet<TValue>();
                result.AddRange(evaluatedItems);
            }
        }

        return result ?? (IReadOnlyCollection<TValue>)Array.Empty<TValue>();
    }

}
