using System.Diagnostics;
using System.Runtime.CompilerServices;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class ValidationResultsComposer
{
    public static ValidationResult Compose<TValidator>(ref TValidator validator, OutputFormat outputFormat) where TValidator : struct, IValidator
    {
        var context = new ValidationCompositionContext(outputFormat);
        validator.CollectValidationResults(ref context);
        return context.BuildFinalResult(validator.Result);
    }

    public static ValidationResult Compose(ref BodyJsonSchema.Validator validator, OutputFormat outputFormat)
    {
        var context = new ValidationCompositionContext(outputFormat);
        validator.CollectValidationResults(ref context);
        validator.CollectAnnotations(ref context);
        return context.BuildFinalResult(validator.Result);
    }
}

internal ref struct ValidationCompositionContext
{
    private readonly OutputFormat _outputFormat;
    private ImmutableValidationErrorCollection.Builder _errorCollectionBuilder;
    private ImmutableDoubleEndedLinkedList<Annotation>.Builder _annotationListBuilder;
    private ValidationResult? _fastResult;

    public ValidationCompositionContext(OutputFormat outputFormat)
    {
        _errorCollectionBuilder = new ImmutableValidationErrorCollection.Builder();
        _annotationListBuilder = new ImmutableDoubleEndedLinkedList<Annotation>.Builder();
        _outputFormat = outputFormat;
    }

    public bool ReportValidationResult(ValidationResult iterationResult, ValidationResult? fastResult)
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
            _errorCollectionBuilder.AddChildCollection(iterationResult.ValidationErrorsList);
            _annotationListBuilder.AddRange(iterationResult.AnnotationList);
        }

        return true;
    }

    public void ReportAnnotation(Annotation annotation)
    {
        _annotationListBuilder.Add(annotation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldAnnotate(bool isValidEventually)
    {
        return isValidEventually && _outputFormat == OutputFormat.List;
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

            return new ValidationResult(true, _errorCollectionBuilder.ToImmutable(), _annotationListBuilder.ToImmutable());
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
                _errorCollectionBuilder.SetCurrent(resultTuple.CurError);
            }

            errorCollection = _errorCollectionBuilder.ToImmutable();
        }

        return new ValidationResult(false, errorCollection, ImmutableDoubleEndedLinkedList<Annotation>.Empty);
    }
}