using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("unevaluatedProperties")]
[JsonConverter(typeof(SingleSchemaJsonConverter<UnevaluatedPropertiesKeyword>))]
public class UnevaluatedPropertiesKeyword : ValidationKeywordBase, ISchemaContainerElement, ISingleSubSchema, IPropertyEvaluator
{
    public JsonSchema Schema { get; init; } = null!;
    public IEnumerable<IPropertyEvaluator> PropertyEvaluators { get; set; } = null!;
    
    public override ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        HashSet<string> evaluatedProperties = FindEvaluatedPropertiesByOtherEvaluators(instance, options);

        options.ValidationPathStack.PushRelativeLocation(Name);

        ValidationResult result = ValidateInternal(evaluatedProperties, instance, options);

        options.ValidationPathStack.PopRelativeLocation();

        return result;
    }

    private ValidationResult ValidateInternal(HashSet<string> evaluatedProperties, JsonInstanceElement instance, JsonSchemaOptions options)
    {
        foreach (JsonInstanceProperty instanceProperty in instance.EnumerateObject())
        {
            if (!evaluatedProperties.Contains(instanceProperty.Name))
            {
                ValidationResult result = Schema.Validate(instanceProperty.Value, options);
                if (!result.IsValid)
                {
                    return result;
                }
            }
        }

        return ValidationResult.ValidResult;
    }

    public IReadOnlyCollection<string> GetEvaluatedProperties(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<string>();
        }

        HashSet<string> evaluatedProperties = FindEvaluatedPropertiesByOtherEvaluators(instance, options);

        var propertyEvaluator = new PropertyEvaluator(this, evaluatedProperties, instance);
        return propertyEvaluator.GetEvaluatedPropertiesWithLocation(options);
    }

    private readonly struct PropertyEvaluator : IPropertyCoreEvaluator
    {
        private readonly UnevaluatedPropertiesKeyword _unevaluatedPropertiesKeyword;
        private readonly HashSet<string> _evaluatedProperties;
        private readonly JsonInstanceElement _instance;

        public PropertyEvaluator(UnevaluatedPropertiesKeyword unevaluatedPropertiesKeyword, HashSet<string> evaluatedProperties, JsonInstanceElement instance)
        {
            _unevaluatedPropertiesKeyword = unevaluatedPropertiesKeyword;
            _evaluatedProperties = evaluatedProperties;
            _instance = instance;
        }

        public string Name => _unevaluatedPropertiesKeyword.Name;

        public IReadOnlyCollection<string> GetEvaluatedProperties(JsonSchemaOptions options)
        {
            var result = new HashSet<string>();

            foreach (JsonInstanceProperty instanceProperty in _instance.EnumerateObject())
            {
                string propertyName = instanceProperty.Name;

                if (!_evaluatedProperties.Contains(propertyName))
                {
                    if (options.OutputFormat == OutputFormat.FailFast)
                    {
                        Debug.Assert(_unevaluatedPropertiesKeyword.Schema.Validate(instanceProperty.Value, options).IsValid);
                    }
                    else
                    {
                        if (!_unevaluatedPropertiesKeyword.Schema.Validate(instanceProperty.Value, options).IsValid)
                        {
                            break;
                        }
                    }

                    result.Add(propertyName);
                }
            }

            return result;
        }
    }

    private HashSet<string> FindEvaluatedPropertiesByOtherEvaluators(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var evaluatedProperties = new HashSet<string>();

        foreach (IPropertyEvaluator propertyEvaluator in PropertyEvaluators)
        {
            foreach (string evaluatedProperty in propertyEvaluator.GetEvaluatedProperties(instance, options))
            {
                evaluatedProperties.Add(evaluatedProperty);
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

public interface IPropertyEvaluator
{
    IReadOnlyCollection<string> GetEvaluatedProperties(JsonInstanceElement instance, JsonSchemaOptions options);
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