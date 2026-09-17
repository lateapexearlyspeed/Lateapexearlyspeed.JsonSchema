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
internal class AnyOfKeyword : ValidationKeywordBase, ISubSchemaCollection, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable, IPropertyEvaluator
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
            var evaluatedProperties = new HashSet<string>();

            foreach (JsonSchema subSchema in _anyOfKeyword._subSchemas)
            {
                if (subSchema is not IPropertyEvaluator propertyEvaluator)
                {
                    continue;
                }

                ValidationResult result = subSchema.Validate(_instance, options);
                if (result.IsValid)
                {
                    evaluatedProperties.AddRange(propertyEvaluator.GetEvaluatedProperties(_instance, options));
                }
            }

            return evaluatedProperties;
        }
    }

    public IReadOnlyCollection<string> GetEvaluatedProperties(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        var propertyEvaluator = new PropertyEvaluator(this, instance);
        return propertyEvaluator.GetEvaluatedPropertiesWithLocation(options);
    }
}
