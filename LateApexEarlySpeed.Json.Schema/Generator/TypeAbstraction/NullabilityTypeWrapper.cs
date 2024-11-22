using System.Diagnostics;
using System.Reflection;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class NullabilityTypeWrapper : IType
{
    private readonly NullabilityType _nullabilityType;

    public NullabilityTypeWrapper(NullabilityType nullabilityType)
    {
        _nullabilityType = nullabilityType;
    }

    public NullabilityState NullabilityState => _nullabilityType.NullabilityState;

    public Type Type => _nullabilityType.Type;

    public IType[] GenericTypeArguments => _nullabilityType.GenericTypeArguments.Select(arg => new NullabilityTypeWrapper(arg)).ToArray<IType>();

    public IType GetArrayElementType()
    {
        if (!Type.IsArray || !Type.HasElementType)
        {
            throw new InvalidOperationException($"Current type: {Type.FullName} is not an array type with element");
        }

        NullabilityType? elementType = _nullabilityType.GetArrayElementType();
        Debug.Assert(elementType is not null);

        return new NullabilityTypeWrapper(elementType);
    }

    public IPropertyInfo[] GetProperties(BindingFlags bindingAttr)
    {
        return _nullabilityType.GetProperties(bindingAttr).Select(prop => new NullabilityPropertyInfoWrapper(prop)).ToArray<IPropertyInfo>();
    }

    public IFieldInfo[] GetFields(BindingFlags bindingAttr)
    {
        return _nullabilityType.GetFields(bindingAttr).Select(field => new NullabilityFieldInfoWrapper(field)).ToArray<IFieldInfo>();
    }

    public IMethodInfo[] GetMethods(BindingFlags bindingAttr)
    {
        return _nullabilityType.GetMethods(bindingAttr).Select(method => new NullabilityMethodInfoWrapper(method)).ToArray<IMethodInfo>();
    }
}