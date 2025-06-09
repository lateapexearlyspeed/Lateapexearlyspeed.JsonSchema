using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class ValidationResultsComposer
{
    public static ValidationResult Compose(IValidator validator, OutputFormat outputFormat)
    {
        var validationErrorsBuilder = new ImmutableValidationErrorCollection.Builder();

        foreach (ValidationResult validationResult in validator.EnumerateValidationResults())
        {
            if (outputFormat == OutputFormat.FailFast && validator.CanFinishFast(out ValidationResult? fastResult))
            {
                return fastResult;
            }

            if (outputFormat == OutputFormat.List)
            {
                validationErrorsBuilder.AddChildCollection(validationResult.ValidationErrorsList);
            }
        }

        ResultTuple resultTuple = validator.Result;

        if (resultTuple.IsValid)
        {
            if (outputFormat == OutputFormat.FailFast)
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(true, validationErrorsBuilder.ToImmutable());
        }

        ImmutableValidationErrorCollection errorCollection;

        if (outputFormat == OutputFormat.FailFast)
        {
            Debug.Assert(resultTuple.CurError is not null);
            errorCollection = new ImmutableValidationErrorCollection(resultTuple.CurError);
        }
        else
        {
            if (resultTuple.CurError is not null)
            {
                validationErrorsBuilder.SetCurrent(resultTuple.CurError);
            }
            
            errorCollection = validationErrorsBuilder.ToImmutable();
        }

        return new ValidationResult(false, errorCollection);
    }
}