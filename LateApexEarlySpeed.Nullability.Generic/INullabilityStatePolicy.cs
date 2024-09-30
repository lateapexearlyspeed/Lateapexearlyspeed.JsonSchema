using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

internal interface INullabilityStatePolicy
{
    NullabilityState FindState(NullabilityInfo nullabilityInfo);
}