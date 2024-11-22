using System.Reflection;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class NullabilityPropertyInfoWrapper : IPropertyInfo
{
    private readonly NullabilityPropertyInfo _propertyInfo;

    public NullabilityPropertyInfoWrapper(NullabilityPropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }

    public MemberInfo MemberInfo => _propertyInfo;
    public IType PropertyType => new NullabilityTypeWrapper(_propertyInfo.NullabilityPropertyType);
}