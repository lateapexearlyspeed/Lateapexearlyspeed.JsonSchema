using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class ConditionalValidator : ISchemaContainerValidationNode
{
    public const string IfKeywordName = "if";
    public const string ThenKeywordName = "then";
    public const string ElseKeywordName = "else";

    private readonly JsonSchema _predictEvaluator;
    private readonly JsonSchema _positiveValidator;
    private readonly JsonSchema _negativeValidator;

    public ConditionalValidator(JsonSchema predictEvaluator, JsonSchema? positiveValidator, JsonSchema? negativeValidator)
    {
        _predictEvaluator = predictEvaluator;
        _predictEvaluator.Name = IfKeywordName;

        _positiveValidator = positiveValidator ?? BooleanJsonSchema.True;
        _positiveValidator.Name = ThenKeywordName;

        _negativeValidator = negativeValidator ?? BooleanJsonSchema.True;
        _negativeValidator.Name = ElseKeywordName;
    }

    public ValidationResult Validate(JsonElement instance, JsonSchemaOptions options)
    {
        return _predictEvaluator.Validate(instance, options).IsValid
            ? _positiveValidator.Validate(instance, options)
            : _negativeValidator.Validate(instance, options);
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        switch (name)
        {
            case IfKeywordName:
                return _predictEvaluator;
            case ThenKeywordName:
                return _positiveValidator;
            case ElseKeywordName:
                return _negativeValidator;
            default:
                return null;
        }
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return _predictEvaluator;
        yield return _positiveValidator;
        yield return _negativeValidator;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}