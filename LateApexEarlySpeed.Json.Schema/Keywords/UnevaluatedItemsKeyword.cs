using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("unevaluatedItems")]
[JsonConverter(typeof(SingleSchemaJsonConverter<UnevaluatedItemsKeyword>))]
public class UnevaluatedItemsKeyword : ValidationKeywordBase, ISchemaContainerElement, ISingleSubSchema, IItemEvaluator
{
    private JsonSchema _schema = null!;

    public JsonSchema Schema
    {
        get => _schema;
        init => _schema = value;
    }

    public IEnumerable<IItemEvaluator> ItemEvaluators { get; set; } = null!;
    
    public override ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Array)
        {
            return ValidationResult.ValidResult;
        }

        HashSet<int>? evaluatedItems = FindEvaluatedItemsByOtherEvaluators(instance, options);

        options.ValidationPathStack.PushRelativeLocation(Name);

        ValidationResult result = ValidateInternal(evaluatedItems, instance, options);

        options.ValidationPathStack.PopRelativeLocation();

        return result;
        
    }

    private ValidationResult ValidateInternal(HashSet<int>? evaluatedItems, JsonInstanceElement instance, JsonSchemaOptions options)
    {
        int i = 0;
        foreach (JsonInstanceElement element in instance.EnumerateArray())
        {
            if (evaluatedItems is null || !evaluatedItems.Contains(i))
            {
                ValidationResult validationResult = _schema.Validate(element, options);

                if (!validationResult.IsValid)
                {
                    return validationResult;
                }
            }

            i++;
        }
        
        return ValidationResult.ValidResult;
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

    public IReadOnlyCollection<int> GetEvaluatedItems(JsonInstanceElement instance, JsonSchemaOptions options)
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

public interface IItemEvaluator
{
    IReadOnlyCollection<int> GetEvaluatedItems(JsonInstanceElement instance, JsonSchemaOptions options);
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