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

[Keyword("oneOf")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<OneOfKeyword>))]
internal class OneOfKeyword : ValidationKeywordBase, ISubSchemaCollection, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable, IPropertyEvaluator, IItemEvaluator
{
    private readonly JsonSchema[] _subSchemas = null!;

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
        private readonly OneOfKeyword _oneOfKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        
        private int _validatedSchemaCount;

        public Validator(OneOfKeyword oneOfKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _oneOfKeyword = oneOfKeyword;
            _instance = instance;
            _options = options;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            if (_options.InUnevaluatedPropertiesScope || _options.InUnevaluatedItemsScope)
            {
                byte[] subSchemaStatus = BitmapHelper.CreateBitmap(_oneOfKeyword._subSchemas.Length);
                for (int i = 0; i < _oneOfKeyword._subSchemas.Length; i++)
                {
                    var subSchema = _oneOfKeyword._subSchemas[i];
                    ValidationResult result = subSchema.Validate(_instance, _options);
                    if (result.IsValid)
                    {
                        _validatedSchemaCount++;
                        BitmapHelper.SetBit(subSchemaStatus, i);
                    }

                    bool reportValidationResult = context.ReportValidationResult(result, null);
                    Debug.Assert(reportValidationResult);
                }

                _options.SetSubSchemaStatusBitmap(_oneOfKeyword, _instance.Location, subSchemaStatus);
            }
            else
            {
                foreach (JsonSchema subSchema in _oneOfKeyword._subSchemas)
                {
                    ValidationResult result = subSchema.Validate(_instance, _options);
                    if (result.IsValid)
                    {
                        _validatedSchemaCount++;
                    }

                    ValidationResult? fastResult = null;
                    if (_validatedSchemaCount > 1)
                    {
                        var error = new ValidationError(ResultCode.MoreThanOnePassedSchemaFound, _options.GenerateErrorMessages ? "More than one schema validate instance" : string.Empty, _options.ValidationPathStack, _oneOfKeyword.Name, _instance.Location);
                        fastResult = ValidationResult.SingleErrorFailedResult(error);
                    }

                    if (!context.ReportValidationResult(result, fastResult))
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
                if (_validatedSchemaCount == 0)
                {
                    var error = new ValidationError(ResultCode.AllSubSchemaFailed, _options.GenerateErrorMessages ? "Instance failed validation against all schemas" : string.Empty, _options.ValidationPathStack, _oneOfKeyword.Name, _instance.Location);

                    return ResultTuple.Invalid(error);
                }

                if (_validatedSchemaCount > 1)
                {
                    var error = new ValidationError(ResultCode.MoreThanOnePassedSchemaFound, _options.GenerateErrorMessages ? "More than one schema validate instance" : string.Empty, _options.ValidationPathStack, _oneOfKeyword.Name, _instance.Location);

                    return ResultTuple.Invalid(error);
                }

                Debug.Assert(_validatedSchemaCount == 1);
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

    IReadOnlyCollection<string> IInfoEvaluator<string>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return GetEvaluatedInfoInternal<IPropertyEvaluator, string>(instance, options);
    }

    IReadOnlyCollection<int> IInfoEvaluator<int>.GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return GetEvaluatedInfoInternal<IItemEvaluator, int>(instance, options);
    }

    private IReadOnlyCollection<TValue> GetEvaluatedInfoInternal<TEvaluator, TValue>(JsonInstanceElement instance, JsonSchemaOptions options) where TEvaluator : IInfoEvaluator<TValue>
    {
        HashSet<TValue>? result = null;

        byte[] subSchemaStatus = options.GetSubSchemaStatusBitmap(this, instance.Location);

        for (int i = 0; i < _subSchemas.Length; i++)
        {
            JsonSchema subSchema = _subSchemas[i];
            if (!BitmapHelper.IsBitSet(subSchemaStatus, i) || subSchema is not TEvaluator evaluator)
            {
                continue;
            }

            IReadOnlyCollection<TValue> subSchemaEvaluatedInfos = evaluator.GetEvaluatedInfos(instance, options);
            if (subSchemaEvaluatedInfos.Count != 0)
            {
                result ??= new HashSet<TValue>();
                result.UnionWith(subSchemaEvaluatedInfos);
            }
        }

        return result ?? (IReadOnlyCollection<TValue>)Array.Empty<TValue>();
    }

}
