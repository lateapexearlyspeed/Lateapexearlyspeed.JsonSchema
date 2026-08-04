using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

public class EquivalentResult
{
    private static readonly EquivalentResult SuccessfulResult = new() { Result = true };
    private static readonly EquivalentResult FailedResultWithoutDetails = new();

    private readonly Func<string>? _detailedMessageFactory;

    public bool Result { get; private init; }
    public string? DetailedMessage => _detailedMessageFactory?.Invoke();
    public LinkedListBasedImmutableJsonPointer? ThisLocation { get; private init; }
    public LinkedListBasedImmutableJsonPointer? OtherLocation { get; private init; }

    private EquivalentResult(Func<string>? detailedMessageFactory = null)
    {
        _detailedMessageFactory = detailedMessageFactory;
    }

    public static EquivalentResult Fail(Func<string> detailedMessageFactory, LinkedListBasedImmutableJsonPointer thisLocation, LinkedListBasedImmutableJsonPointer otherLocation)
    {
        return new EquivalentResult(detailedMessageFactory)
        {
            Result = false,
            ThisLocation = thisLocation,
            OtherLocation = otherLocation
        };
    }

    internal static EquivalentResult FailWithoutDetails() => FailedResultWithoutDetails;

    public static EquivalentResult Success() => SuccessfulResult;
}

internal enum ComparisonDetail
{
    ResultOnly,
    IncludeFailureDetails
}