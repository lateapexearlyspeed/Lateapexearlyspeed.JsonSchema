using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SingleSchemaJsonConverter<UnevaluatedPropertiesKeyword>))]
public class UnevaluatedPropertiesKeyword : ValidationKeywordBase, ISchemaContainerElement, ISingleSubSchema, IPropertyEvaluator
{
    public const string Keyword = "unevaluatedProperties";

    public JsonSchema Schema { get; init; } = null!;
    public IPropertyEvaluator[] PropertyEvaluators { private get; set; } = null!;
    
    public override ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        HashSet<string>? evaluatedProperties = FindEvaluatedPropertiesByOtherEvaluators(instance, options);

        options.ValidationPathStack.PushRelativeLocation(Name);

        // Sub schema of this keyword applies to property values, which start new unevaluated scopes.
        bool origPropState = options.EnterUnevaluatedPropertiesScope(false);
        bool origItemState = options.EnterUnevaluatedItemsScope(false);

        ValidationResult result = ValidateInternal(evaluatedProperties, instance, options);

        options.ExitUnevaluatedItemsScope(origItemState);
        options.ExitUnevaluatedPropertiesScope(origPropState);

        options.ValidationPathStack.PopRelativeLocation();

        return result;
    }

    private ValidationResult ValidateInternal(HashSet<string>? evaluatedProperties, JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var validator = new Validator(this, evaluatedProperties, instance, options);
        return ValidationResultsComposer.Compose(ref validator, options.OutputFormat);
    }

    private struct Validator : IValidator
    {
        private readonly UnevaluatedPropertiesKeyword _unevaluatedPropertiesKeyword;
        private readonly HashSet<string>? _evaluatedProperties;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(UnevaluatedPropertiesKeyword unevaluatedPropertiesKeyword, HashSet<string>? evaluatedProperties, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _unevaluatedPropertiesKeyword = unevaluatedPropertiesKeyword;
            _evaluatedProperties = evaluatedProperties;
            _instance = instance;
            _options = options;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            foreach (JsonInstanceProperty instanceProperty in _instance.EnumerateObject())
            {
                if (_evaluatedProperties is null || !_evaluatedProperties.Contains(instanceProperty.Name))
                {
                    ValidationResult result = _unevaluatedPropertiesKeyword.Schema.Validate(instanceProperty.Value, _options);
                    if (!result.IsValid)
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

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }

    /// <summary>
    /// DEAD CODE (kept for interface completeness): this keyword's own <see cref="IPropertyEvaluator"/> implementation
    /// (this method and the nested <see cref="PropertyEvaluator"/> struct) is never reached.
    /// An outer 'unevaluatedProperties' never queries this keyword as an evaluator, because when a <see cref="BodyJsonSchema"/> owns an
    /// 'unevaluatedProperties' keyword, its property evaluator short-circuits to "all instance properties are evaluated" before any evaluator scan.
    /// </summary>
    public IReadOnlyCollection<string> GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<string>();
        }

        HashSet<string>? evaluatedProperties = FindEvaluatedPropertiesByOtherEvaluators(instance, options);

        var propertyEvaluator = new PropertyEvaluator(this, evaluatedProperties, instance);
        return propertyEvaluator.GetEvaluatedPropertiesWithLocation(options);
    }

    private readonly struct PropertyEvaluator : IPropertyCoreEvaluator
    {
        private readonly UnevaluatedPropertiesKeyword _unevaluatedPropertiesKeyword;
        private readonly HashSet<string>? _evaluatedProperties;
        private readonly JsonInstanceElement _instance;

        public PropertyEvaluator(UnevaluatedPropertiesKeyword unevaluatedPropertiesKeyword, HashSet<string>? evaluatedProperties, JsonInstanceElement instance)
        {
            _unevaluatedPropertiesKeyword = unevaluatedPropertiesKeyword;
            _evaluatedProperties = evaluatedProperties;
            _instance = instance;
        }

        public string Name => _unevaluatedPropertiesKeyword.Name;

        public IReadOnlyCollection<string> GetEvaluatedProperties(JsonSchemaOptions options)
        {
            List<string>? result = null;

            foreach (JsonInstanceProperty instanceProperty in _instance.EnumerateObject())
            {
                string propertyName = instanceProperty.Name;

                if (_evaluatedProperties is null || !_evaluatedProperties.Contains(propertyName))
                {
                    result ??= new List<string>();
                    result.Add(propertyName);
                }
            }

            return result ?? (IReadOnlyCollection<string>)Array.Empty<string>();
        }
    }

    private HashSet<string>? FindEvaluatedPropertiesByOtherEvaluators(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        HashSet<string>? evaluatedProperties = null;

        foreach (IPropertyEvaluator propertyEvaluator in PropertyEvaluators)
        {
            IReadOnlyCollection<string> properties = propertyEvaluator.GetEvaluatedProperties(instance, options);
            
            if (properties.Count != 0)
            {
                evaluatedProperties ??= new HashSet<string>();
                evaluatedProperties.AddRange(properties);
            }
        }

        return evaluatedProperties;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        throw new NotImplementedException($"Override {nameof(Validate)} method already");
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
}

public interface IPropertyEvaluator : IInfoEvaluator<string>
{
    IReadOnlyCollection<string> GetEvaluatedProperties(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        return GetEvaluatedInfos(instance, options);
    }
}

internal interface IPropertyCoreEvaluator
{
    string? Name { get; }
    IReadOnlyCollection<string> GetEvaluatedProperties(JsonSchemaOptions options);
}

internal static class PropertyEvaluationLocationDecorator
{
    public static IReadOnlyCollection<string> GetEvaluatedPropertiesWithLocation<TPropertyCoreEvaluator>(this ref TPropertyCoreEvaluator evaluator, JsonSchemaOptions options)
        where TPropertyCoreEvaluator : struct, IPropertyCoreEvaluator
    {
        if (evaluator.Name is not null)
        {
            options.ValidationPathStack.PushRelativeLocation(evaluator.Name);
        }

        IReadOnlyCollection<string> result = evaluator.GetEvaluatedProperties(options);

        if (evaluator.Name is not null)
        {
            options.ValidationPathStack.PopRelativeLocation();
        }

        return result;
    }
}