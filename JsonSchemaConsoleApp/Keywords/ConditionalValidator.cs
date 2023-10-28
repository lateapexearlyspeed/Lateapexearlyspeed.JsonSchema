using System.Text.Json;

namespace JsonSchemaConsoleApp.Keywords;

internal class ConditionalValidator
{
    public ConditionalValidator(JsonSchema? predictEvaluator, JsonSchema? positiveValidator, JsonSchema? negativeValidator)
    {
        if (predictEvaluator is not null)
        {
            PredictEvaluator = predictEvaluator;
            PredictEvaluator.Name = IfKeyword.Keyword;
        }

        PositiveValidator = positiveValidator ?? BooleanJsonSchema.True;
        PositiveValidator.Name = ThenKeyword.Keyword;

        NegativeValidator = negativeValidator ?? BooleanJsonSchema.True;
        NegativeValidator.Name = ElseKeyword.Keyword;
    }

    public JsonSchema? PredictEvaluator { get; }
    public JsonSchema PositiveValidator { get; }
    public JsonSchema NegativeValidator { get; }

    public ValidationResult Validate(JsonElement instance, JsonSchemaOptions options)
    {
        if (PredictEvaluator is null)
        {
            return ValidationResult.ValidResult;
        }

        return PredictEvaluator.Validate(instance, options).IsValid
            ? PositiveValidator.Validate(instance, options)
            : NegativeValidator.Validate(instance, options);
    }

    public JsonSchema? GetSubElement(string name)
    {
        switch (name)
        {
            case IfKeyword.Keyword:
                return PredictEvaluator;
            case ThenKeyword.Keyword:
                return PositiveValidator;
            case ElseKeyword.Keyword:
                return NegativeValidator;
            default:
                return null;
        }
    }
}