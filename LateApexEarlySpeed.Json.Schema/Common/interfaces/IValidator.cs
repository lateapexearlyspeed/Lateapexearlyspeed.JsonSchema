using System.Diagnostics.CodeAnalysis;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

internal interface IValidator
{
    IEnumerable<ValidationResult> EnumerateValidationResults();
    bool CanFinishFast([NotNullWhen(true)] out ValidationResult? validationResult);
    ResultTuple Result { get; }
}

internal readonly ref struct ResultTuple
{
    public static ResultTuple Valid()
    {
        return new ResultTuple(true, null);
    }

    public static ResultTuple WithError(ValidationError curError)
    {
        return new ResultTuple(false, curError);
    }

    private ResultTuple(bool isValid, ValidationError? curError)
    {
        IsValid = isValid;
        CurError = curError;
    }

    [MemberNotNullWhen(false, nameof(CurError))]
    public bool IsValid { get; }

    public ValidationError? CurError { get; }
}