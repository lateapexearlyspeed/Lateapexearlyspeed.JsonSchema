using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class AnnotationBasedNullabilityPolicy : NullabilityPolicy
{
    protected internal override bool UseNullabilityAnnotation => true;

    protected internal override NullabilityState GetNullabilityState(IMemberInfo memberInfo)
    {
        return ((NullabilityTypeWrapper)memberInfo.GetMemberType()).NullabilityState;
    }
}
