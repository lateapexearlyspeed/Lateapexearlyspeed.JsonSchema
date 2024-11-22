using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class PropertyInfoWrapper : IPropertyInfo
{
    private readonly PropertyInfo _propertyInfo;

    public PropertyInfoWrapper(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }

    public MemberInfo MemberInfo => _propertyInfo;
    public IType PropertyType => new TypeWrapper(_propertyInfo.PropertyType);
}