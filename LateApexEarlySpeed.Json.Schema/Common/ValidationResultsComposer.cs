using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class ValidationResultsComposer
{
    public static ValidationResult Compose<TValidator>(ref TValidator validator, OutputFormat outputFormat) where TValidator : struct, IValidator
    {
        var context = new ValidationCompositionContext(outputFormat);
        validator.CollectValidationResults(ref context);
        return context.BuildFinalResult(validator.Result);
    }
}

internal ref struct ValidationCompositionContext
{
    private readonly OutputFormat _outputFormat;
    private ImmutableValidationErrorCollection.Builder _builder;
    private ValidationResult? _fastResult;

    public ValidationCompositionContext(OutputFormat outputFormat)
    {
        _builder = new ImmutableValidationErrorCollection.Builder();
        _outputFormat = outputFormat;
    }

    public bool Report(ValidationResult iterationResult, ValidationResult? fastResult)
    {
        if (_fastResult is not null)
        {
            return false;
        }

        if (_outputFormat == OutputFormat.FailFast && fastResult is not null)
        {
            _fastResult = fastResult;
            return false;
        }

        if (_outputFormat == OutputFormat.List)
        {
            _builder.AddChildCollection(iterationResult.ValidationErrorsList);
        }

        return true;
    }

    public ValidationResult BuildFinalResult(ResultTuple resultTuple)
    {
        if (_fastResult is not null)
        {
            return _fastResult;
        }

        if (resultTuple.IsValid)
        {
            if (_outputFormat == OutputFormat.FailFast)
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(true, _builder.ToImmutable());
        }

        ImmutableValidationErrorCollection errorCollection;

        if (_outputFormat == OutputFormat.FailFast)
        {
            Debug.Assert(resultTuple.CurError is not null);
            errorCollection = new ImmutableValidationErrorCollection(resultTuple.CurError);
        }
        else
        {
            if (resultTuple.CurError is not null)
            {
                _builder.SetCurrent(resultTuple.CurError);
            }

            errorCollection = _builder.ToImmutable();
        }

        return new ValidationResult(false, errorCollection);
    }
}