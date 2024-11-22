using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public abstract class NullabilityPolicy
{
    public static NullabilityPolicy BasedOnAttribute { get; } = new AttributeBasedNullabilityPolicy();
    public static NullabilityPolicy BasedOnNullabilityAnnotation { get; } = new AnnotationBasedNullabilityPolicy();

    protected internal virtual bool UseNullabilityAnnotation => false;
    protected internal abstract NullabilityState GetNullabilityState(IMemberInfo memberInfo);
}