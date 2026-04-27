using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("oneOf")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<OneOfKeyword>))]
internal class OneOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement, IJsonSchemaResourceNodesCleanable
{
    private readonly JsonSchema[] _subSchemas = null!;

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
        private readonly OneOfKeyword _oneOfKeyword;
        private readonly JsonInstanceElement _instance;
        private readonly JsonSchemaOptions _options;
        
        private int _validatedSchemaCount;

        public Validator(OneOfKeyword oneOfKeyword, JsonInstanceElement instance, JsonSchemaOptions options)
        {
            _oneOfKeyword = oneOfKeyword;
            _instance = instance;
            _options = options;
        }

        public void CollectValidationResults(ref ValidationCompositionContext context)
        {
            foreach (JsonSchema subSchema in _oneOfKeyword._subSchemas)
            {
                ValidationResult result = subSchema.Validate(_instance, _options);
                if (result.IsValid)
                {
                    _validatedSchemaCount++;
                }

                ValidationResult? fastResult = null;
                if (_validatedSchemaCount > 1)
                {
                    var error = new ValidationError(ResultCode.MoreThanOnePassedSchemaFound, "More than one schema validate instance", _options.ValidationPathStack, _oneOfKeyword.Name, _instance.Location);
                    fastResult = ValidationResult.SingleErrorFailedResult(error);
                }

                if (!context.Report(result, fastResult))
                {
                    break;
                }
            }
        }

        public ResultTuple Result
        {
            get
            {
                if (_validatedSchemaCount == 0)
                {
                    var error = new ValidationError(ResultCode.AllSubSchemaFailed, "Instance failed validation against all schemas", _options.ValidationPathStack, _oneOfKeyword.Name, _instance.Location);

                    return ResultTuple.Invalid(error);
                }

                if (_validatedSchemaCount > 1)
                {
                    var error = new ValidationError(ResultCode.MoreThanOnePassedSchemaFound, "More than one schema validate instance", _options.ValidationPathStack, _oneOfKeyword.Name, _instance.Location);

                    return ResultTuple.Invalid(error);
                }

                Debug.Assert(_validatedSchemaCount == 1);
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
}
