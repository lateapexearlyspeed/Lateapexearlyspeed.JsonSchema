using System.Diagnostics.CodeAnalysis;
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
internal class AnyOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable
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
        return ValidationResultsComposer.Compose(new Validator(this, instance, options), options.OutputFormat);
    }

    private class Validator : IValidator
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

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonSchema subSchema in _anyOfKeyword.SubSchemas)
            {
                ValidationResult result = subSchema.Validate(_instance, _options);
                if (result.IsValid)
                {
                    _fastReturnResult = result;
                }

                yield return result;
            }
        }

        public bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult)
        {
            validationResult = _fastReturnResult;

            return _fastReturnResult is not null;
        }

        public ResultTuple Result
        {
            get
            {
                if (_fastReturnResult is null)
                {
                    var curError = new ValidationError(ResultCode.AllSubSchemaFailed, ErrorMessage(), _options.ValidationPathStack, _anyOfKeyword.Name, _instance.Location);

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
}
