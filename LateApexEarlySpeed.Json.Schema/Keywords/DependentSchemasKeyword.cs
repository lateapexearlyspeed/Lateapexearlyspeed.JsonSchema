using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("dependentSchemas")]
[JsonConverter(typeof(DependentSchemasKeywordJsonConverter))]
internal class DependentSchemasKeyword : KeywordBase, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable
{
    private readonly Dictionary<string, JsonSchema> _dependentSchemas;

    public DependentSchemasKeyword(IDictionary<string, JsonSchema> dependentSchemas, bool propertyNameIgnoreCase)
    {
        _dependentSchemas = new Dictionary<string, JsonSchema>(dependentSchemas, propertyNameIgnoreCase ? StringComparer.OrdinalIgnoreCase : null);

        foreach (var (propertyName, schema) in DependentSchemas)
        {
            schema.Name = propertyName;
        }
    }

    public IReadOnlyDictionary<string, JsonSchema> DependentSchemas => _dependentSchemas;

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(_dependentSchemas, instance, options), options.OutputFormat);
    }

    internal class Validator : IValidator
    {
        private readonly IReadOnlyDictionary<string, JsonSchema> _dependentSchemas;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        
        private ValidationResult? _fastReturnResult;

        public Validator(IReadOnlyDictionary<string, JsonSchema> dependentSchemas, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _dependentSchemas = dependentSchemas;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonInstanceProperty instanceProperty in _instance.EnumerateObject())
            {
                if (_dependentSchemas.TryGetValue(instanceProperty.Name, out JsonSchema? subSchema))
                {
                    ValidationResult result = subSchema.Validate(_instance, _options);
                    if (!result.IsValid)
                    {
                        _fastReturnResult = result;
                    }

                    yield return result;
                }
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
        return DependentSchemas.GetValueOrDefault(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return DependentSchemas.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        foreach ((string name, JsonSchema schema) in _dependentSchemas)
        {
            if (schema is BodyJsonSchema bodyJsonSchema)
            {
                BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _dependentSchemas[name] = newSchema);
            }
        }
    }
}