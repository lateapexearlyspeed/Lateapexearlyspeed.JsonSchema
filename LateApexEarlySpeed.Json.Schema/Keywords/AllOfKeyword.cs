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

[Keyword(Keyword)]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<AllOfKeyword>))]
internal class AllOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement
{
    public const string Keyword = "allOf";

    private readonly JsonSchema[] _subSchemas = null!;

    public AllOfKeyword()
    {
    }

    public AllOfKeyword(IEnumerable<JsonSchema> subSchemas)
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
        private readonly AllOfKeyword _allOfKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        
        private ValidationResult? _fastReturnResult;

        public Validator(AllOfKeyword allOfKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _allOfKeyword = allOfKeyword;
            _instance = instance;
            _options = options;
        }

        public IEnumerable<ValidationResult> EnumerateValidationResults()
        {
            foreach (JsonSchema subSchema in _allOfKeyword.SubSchemas)
            {
                ValidationResult result = subSchema.Validate(_instance, _options);
                if (!result.IsValid)
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
                    return ResultTuple.Valid();
                }

                ValidationError curError = new ValidationError(ResultCode.FailedInSubSchema, ValidationError.ErrorMessageForFailedInSubSchema, _options.ValidationPathStack, _allOfKeyword.Name, _instance.Location);

                return ResultTuple.WithError(curError);
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
}

