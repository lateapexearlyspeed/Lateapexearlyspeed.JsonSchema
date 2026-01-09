using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("properties")]
[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
internal class PropertiesKeyword : KeywordBase, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable
{
    private readonly Dictionary<string, JsonSchema> _propertiesSchemas;

    public IReadOnlyDictionary<string, JsonSchema> PropertiesSchemas => _propertiesSchemas;

    public PropertiesKeyword(IDictionary<string, JsonSchema> propertiesSchemas, bool propertyNameIgnoreCase)
    {
        _propertiesSchemas = new Dictionary<string, JsonSchema>(propertiesSchemas, propertyNameIgnoreCase ? StringComparer.OrdinalIgnoreCase : null);

        foreach (var (propName, schema) in _propertiesSchemas)
        {
            schema.Name = propName;
        }
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (instance.ValueKind != JsonValueKind.Object)
        {
            return ValidationResult.ValidResult;
        }

        return ValidationResultsComposer.Compose(new Validator(this, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
    {
        private readonly PropertiesKeyword _propertiesKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationResult? _fastReturnResult;

        public Validator(PropertiesKeyword propertiesKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _propertiesKeyword = propertiesKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonInstanceProperty instanceProperty in _instance.EnumerateObject())
            {
                if (_propertiesKeyword.PropertiesSchemas.TryGetValue(instanceProperty.Name, out JsonSchema? schema))
                {
                    ValidationResult result = schema.Validate(instanceProperty.Value, _options);
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
            validationResult = _fastReturnResult;

            return _fastReturnResult is not null;
        }

        public ResultTuple Result => _fastReturnResult is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return PropertiesSchemas.GetValueOrDefault(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return PropertiesSchemas.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public bool ContainsPropertyName(string propertyName)
    {
        return PropertiesSchemas.ContainsKey(propertyName);
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        foreach ((string property, JsonSchema schema) in _propertiesSchemas)
        {
            if (schema is BodyJsonSchema bodyJsonSchema)
            {
                BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _propertiesSchemas[property] = newSchema);
            }
        }
    }
}
