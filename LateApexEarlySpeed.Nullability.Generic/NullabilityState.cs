namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// An enum that represents nullability state
/// </summary>
public enum NullabilityState
{
    /// <summary>
    /// Nullability context not enabled (oblivious)
    /// </summary>
    Unknown,
    /// <summary>
    /// Non nullable value or reference type
    /// </summary>
    NotNull,
    /// <summary>
    /// Nullable value or reference type
    /// </summary>
    Nullable
}