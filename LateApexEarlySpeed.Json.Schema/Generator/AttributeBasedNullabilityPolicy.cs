using LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;
using LateApexEarlySpeed.Nullability.Generic;
using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator;

public class AttributeBasedNullabilityPolicy : NullabilityPolicy
{
    protected internal override NullabilityState GetNullabilityState(IMemberInfo memberInfo)
    {
        return memberInfo.MemberInfo.GetCustomAttribute<NotNullAttribute>() is null ? NullabilityState.Unknown : NullabilityState.NotNull;
    }
}