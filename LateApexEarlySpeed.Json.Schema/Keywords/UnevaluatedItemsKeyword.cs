using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SingleSchemaJsonConverter<UnevaluatedItemsKeyword>))]
public class UnevaluatedItemsKeyword : ValidationKeywordBase, ISchemaContainerElement, ISingleSubSchema, IItemEvaluator
{
    public const string Keyword = "unevaluatedItems";

    private JsonSchema _schema = null!;

    public JsonSchema Schema
    {
        get => _schema;
        init => _schema = value;
    }

    public IItemEvaluator[] ItemEvaluators { private get; set; } = null!;
    
    public override ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        HashSet<int>? evaluatedItems = FindEvaluatedItemsByOtherEvaluators(instance, options);

        options.ValidationPathStack.PushRelativeLocation(Name);

        // Sub schema of this keyword applies to array items, which start new unevaluated scopes.
        bool origPropState = options.EnterUnevaluatedPropertiesScope(false);
        bool origItemState = options.EnterUnevaluatedItemsScope(false);

        ValidationResult result = ValidateInternal(evaluatedItems, instance, options);

        options.ExitUnevaluatedItemsScope(origItemState);
        options.ExitUnevaluatedPropertiesScope(origPropState);

        options.ValidationPathStack.PopRelativeLocation();

        return result;
    }

    private ValidationResult ValidateInternal(HashSet<int>? evaluatedItems, JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var validator = new Validator(this, evaluatedItems, instance, options);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    private struct Validator : IValidator
    {
        private readonly UnevaluatedItemsKeyword _unevaluatedItemsKeyword;
        private readonly HashSet<int>? _evaluatedItems;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(UnevaluatedItemsKeyword unevaluatedItemsKeyword, HashSet<int>? evaluatedItems, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _unevaluatedItemsKeyword = unevaluatedItemsKeyword;
            _evaluatedItems = evaluatedItems;
            _instance = instance;
            _options = options;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            int i = 0;
            foreach (JsonInstanceElement element in _instance.EnumerateArray())
            {
                if (_evaluatedItems is null || !_evaluatedItems.Contains(i))
                {
                    ValidationResult validationResult = _unevaluatedItemsKeyword._schema.Validate(element, _options);
                    if (!validationResult.IsValid)
                    {
                        _fastReturnResult = validationResult;
                    }

                    if (!context.ReportValidationResult(validationResult, _fastReturnResult))
                    {
                        break;
                    }
                }

                i++;
            }
        }

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }

    private HashSet<int>? FindEvaluatedItemsByOtherEvaluators(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        HashSet<int>? items = null;

        foreach (IItemEvaluator evaluator in ItemEvaluators)
        {
            IReadOnlyCollection<int> evaluatedItems = evaluator.GetEvaluatedItems(instance, options);
            if (evaluatedItems.Count > 0)
            {
                items ??= new HashSet<int>();
                items.AddRange(evaluatedItems);
            }
        }

        return items;
    }

    /// <summary>
    /// DEAD CODE (kept for interface completeness): this keyword's own <see cref="IItemEvaluator"/> implementation
    /// (this method) is never reached. An outer 'unevaluatedItems'
    /// never queries this keyword as an evaluator, because when a <see cref="BodyJsonSchema"/> owns an 'unevaluatedItems'
    /// keyword, its item evaluator short-circuits to "all array items are evaluated" before any evaluator scan.
    /// </summary>
    public IReadOnlyCollection<int> GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<int>();
        }

        HashSet<int>? evaluatedItems = FindEvaluatedItemsByOtherEvaluators(instance, options);
        List<int>? result = null;

        int arrayLength = instance.GetArrayLength();

        for (int i = 0; i < arrayLength; i++)
        {
            if (evaluatedItems is null || !evaluatedItems.Contains(i))
            {
                result ??= new List<int>();
                result.Add(i);
            }
        }

        return result ?? (IReadOnlyCollection<int>)Array.Empty<int>();
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return Schema.GetSubElement(name);
    }

    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return Schema;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return Schema;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        throw new NotImplementedException($"Override {nameof(Validate)} method already");
    }
}

public interface IItemEvaluator : IInfoEvaluator<int>
{
    IReadOnlyCollection<int> GetEvaluatedItems(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return GetEvaluatedInfos(instance, options);
    }
}

internal interface IItemCoreEvaluator
{
    string? Name { get; }
    IReadOnlyCollection<int> GetEvaluatedItems(JsonSchemaOptions options);
}

internal static class ItemEvaluationLocationDecorator
{
    public static IReadOnlyCollection<int> GetEvaluatedItemsWithLocation<TItemCoreEvaluator>(this ref TItemCoreEvaluator evaluator, JsonSchemaOptions options)
        where TItemCoreEvaluator : struct, IItemCoreEvaluator
    {
        if (evaluator.Name is not null)
        {
            options.ValidationPathStack.PushRelativeLocation(evaluator.Name);
        }

        IReadOnlyCollection<int> result = evaluator.GetEvaluatedItems(options);

        if (evaluator.Name is not null)
        {
            options.ValidationPathStack.PopRelativeLocation();
        }

        return result;
    }
}