using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("propertyNames")]
[JsonConverter(typeof(SingleSchemaJsonConverter<PropertyNamesKeyword>))]
internal class PropertyNamesKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema, IJsonSchemaResourceNodesCleanable
{
    private JsonSchema _schema = null!;

    public JsonSchema Schema
    {
        get => _schema;
        init => _schema = value;
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
        private readonly PropertyNamesKeyword _propertyNamesKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;

        private ValidationError? _fastReturnError;

        public Validator(PropertyNamesKeyword propertyNamesKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _propertyNamesKeyword = propertyNamesKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonInstanceProperty jsonProperty in _instance.EnumerateObject())
            {
                ValidationResult validationResult = _propertyNamesKeyword.Schema.Validate(new JsonInstanceElement(JsonSerializer.SerializeToElement(jsonProperty.Name), ImmutableJsonPointer.Empty), _options);
                if (!validationResult.IsValid)
                {
                    _fastReturnError = new ValidationError(ResultCode.InvalidPropertyName, ErrorMessage(jsonProperty.Name), _options.ValidationPathStack, _propertyNamesKeyword.Name, _instance.Location);
                }

                yield return validationResult;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            if (_fastReturnError is null)
            {
                validationResult = null;
                return false;
            }

            validationResult = ValidationResult.SingleErrorFailedResult(_fastReturnError);
            return true;
        }

        public ResultTuple Result => _fastReturnError is null ? ResultTuple.Valid() : ResultTuple.Invalid(null);
    }

    public static string ErrorMessage(string propertyName)
    {
        return $"Found invalid property name: {propertyName}";
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return Schema.GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return Schema;
    }

    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return Schema;
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        if (_schema is BodyJsonSchema bodyJsonSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _schema = newSchema);
        }
    }
}
