namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

internal interface IValidator
{
    void CollectValidationResults(ref ValidationCompositionContext context);

    /// <remarks>
    /// NOTE for implementations: there are some cases in which <see cref="ResultTuple.CurError"/> of <see cref="Result"/> Must NOT null.
    /// Check <see cref="ValidationResultsComposer"/> code to understand.
    /// </remarks>
    ResultTuple Result { get; }
}

internal readonly ref struct ResultTuple
{
    public static ResultTuple Valid()
    {
        return new ResultTuple(true, null);
    }

    public static ResultTuple Invalid(ValidationError? curError)
    {
        return new ResultTuple(false, curError);
    }

    private ResultTuple(bool isValid, ValidationError? curError)
    {
        IsValid = isValid;
        CurError = curError;
    }

    public bool IsValid { get; }

    public ValidationError? CurError { get; }
}