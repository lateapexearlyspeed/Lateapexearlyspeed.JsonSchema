﻿using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class ConditionalValidator : ISchemaContainerValidationNode
{
    public const string IfKeywordName = "if";
    public const string ThenKeywordName = "then";
    public const string ElseKeywordName = "else";

    public JsonSchema? PredictEvaluator { get; }
    public JsonSchema PositiveValidator { get; }
    public JsonSchema NegativeValidator { get; }

    public ConditionalValidator(JsonSchema? predictEvaluator, JsonSchema? positiveValidator, JsonSchema? negativeValidator)
    {
        PredictEvaluator = predictEvaluator;
        if (PredictEvaluator is not null)
        {
            PredictEvaluator.Name = IfKeywordName;
        }

        PositiveValidator = positiveValidator ?? BooleanJsonSchema.True;
        PositiveValidator.Name = ThenKeywordName;

        NegativeValidator = negativeValidator ?? BooleanJsonSchema.True;
        NegativeValidator.Name = ElseKeywordName;
    }

    public ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        if (PredictEvaluator is null)
        {
            return ValidationResult.ValidResult;
        }

        if (options.OutputFormat == OutputFormat.FailFast)
        {
            return PredictEvaluator.Validate(instance, options).IsValid
                ? PositiveValidator.Validate(instance, options)
                : NegativeValidator.Validate(instance, options);
        }

        var errorsBuilder = new ImmutableValidationErrorCollection.Builder();

        ValidationResult predictResult = PredictEvaluator.Validate(instance, options);
        errorsBuilder.AddChildCollection(predictResult.ValidationErrorsList);

        if (predictResult.IsValid)
        {
            ValidationResult positiveResult = PositiveValidator.Validate(instance, options);
            errorsBuilder.AddChildCollection(positiveResult.ValidationErrorsList);

            return new ValidationResult(positiveResult.IsValid, errorsBuilder.ToImmutable());
        }

        ValidationResult negativeResult = NegativeValidator.Validate(instance, options);
        errorsBuilder.AddChildCollection(negativeResult.ValidationErrorsList);

        return new ValidationResult(negativeResult.IsValid, errorsBuilder.ToImmutable());
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        switch (name)
        {
            case IfKeywordName:
                return PredictEvaluator;
            case ThenKeywordName:
                return PositiveValidator;
            case ElseKeywordName:
                return NegativeValidator;
            default:
                return null;
        }
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        if (PredictEvaluator is not null)
        {
            yield return PredictEvaluator;
        }
        
        yield return PositiveValidator;
        yield return NegativeValidator;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}