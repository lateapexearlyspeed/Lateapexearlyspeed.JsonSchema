using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class NullabilityTypeInfo
{
    /// <summary>
    /// Nullability info of array element of root type, if root type itself is array type.
    /// </summary>
    public NullabilityElement? ArrayElementNullability { get; set; }

    /// <summary>
    /// Nullability info of generic type arguments of root type, if root type itself is generic type.
    /// </summary>
    public NullabilityElement[]? GenericTypeArgumentsNullabilities { get; set; }

    /// <summary>
    /// Gets or sets a value that specifies the policy used to decide reference type property or field's nullability on an object, such as by <see cref="NotNullAttribute"/> or by nullability annotation.
    /// </summary>
    public NullabilityPolicy ReferenceTypeNullabilityPolicy { get; set; } = NullabilityPolicy.BasedOnAttribute;
}