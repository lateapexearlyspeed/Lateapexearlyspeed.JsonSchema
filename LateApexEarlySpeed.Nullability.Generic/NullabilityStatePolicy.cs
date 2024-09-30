using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

internal class NullabilityStatePolicy : INullabilityStatePolicy
{
    public NullabilityState FindState(NullabilityInfo nullabilityInfo)
    {
        if (nullabilityInfo.ReadState == NullabilityState.Unknown)
        {
            return nullabilityInfo.WriteState;
        }

        if (nullabilityInfo.WriteState == NullabilityState.Unknown)
        {
            return nullabilityInfo.ReadState;
        }

        return nullabilityInfo.ReadState;
    }
}