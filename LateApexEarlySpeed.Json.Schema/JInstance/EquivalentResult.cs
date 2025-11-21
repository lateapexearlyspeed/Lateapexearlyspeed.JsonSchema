using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.JInstance;

public class EquivalentResult
{
    public bool Result { get; private init; }
    public string? DetailedMessage { get; private init; }
    public LinkedListBasedImmutableJsonPointer? ThisLocation { get; private init; }
    public LinkedListBasedImmutableJsonPointer? OtherLocation { get; private init; }

    private EquivalentResult()
    {
    }

    public static EquivalentResult Fail(string detailedMessage, LinkedListBasedImmutableJsonPointer thisLocation, LinkedListBasedImmutableJsonPointer otherLocation)
    {
        return new EquivalentResult
        {
            Result = false,
            DetailedMessage = detailedMessage,
            ThisLocation = thisLocation,
            OtherLocation = otherLocation
        };
    }

    public static EquivalentResult Success() => new() { Result = true };
}