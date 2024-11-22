using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class NullabilityTypeInfo
{
    public NullabilityElement? ArrayElementNullability { get; set; }
    public NullabilityElement[]? GenericTypeArgumentsNullabilities { get; set; }

    public NullabilityPolicy ReferenceTypeNullabilityPolicy { get; set; } = NullabilityPolicy.BasedOnAttribute;
}