using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public abstract class NullabilityPolicy
{
    /// <summary>
    /// Gets the nullability policy for <see cref="NotNullAttribute"/> based policy
    /// </summary>
    public static NullabilityPolicy BasedOnAttribute { get; } = new AttributeBasedNullabilityPolicy();

    /// <summary>
    /// Gets the nullability policy for nullability annotation based policy
    /// </summary>
    public static NullabilityPolicy BasedOnNullabilityAnnotation { get; } = new AnnotationBasedNullabilityPolicy();

    protected internal virtual bool UseNullabilityAnnotation => false;
    protected internal abstract NullabilityState GetNullabilityState(IMemberInfo memberInfo);
}