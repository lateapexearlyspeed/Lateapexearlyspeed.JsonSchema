using System.Globalization;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="PropertyInfo"/> and its annotated nullability info
/// </summary>
public partial class NullabilityPropertyInfo
{
    private readonly NullabilityInfoContext _context = new();

    private readonly PropertyInfo _propertyInfo;

    private readonly Type? _propertyTypeInGenericDefType;

    private readonly NullabilityType _declaringType;

    protected internal NullabilityPropertyInfo(PropertyInfo propertyInfo, Type? propertyTypeInGenericDefType, NullabilityType declaringType)
    {
        _propertyInfo = propertyInfo;
        _propertyTypeInGenericDefType = propertyTypeInGenericDefType;
        _declaringType = declaringType;
    }

    /// <summary>
    /// Gets the nullability type of current property.
    /// </summary>
    public NullabilityType NullabilityPropertyType
    {
        get
        {
            Type propertyType = _propertyInfo.PropertyType;
            NullabilityInfo origNullabilityInfo = _context.Create(_propertyInfo);
            NullabilityElement nullabilityElement = NullabilityElement.Create(_propertyTypeInGenericDefType ?? propertyType, _declaringType, origNullabilityInfo);

            return new NullabilityType(propertyType, nullabilityElement);
        }
    }

    // todo: need to consider nullable related attribute later
    /// <summary>
    /// Gets the nullability read state of current property.
    /// </summary>
    public NullabilityState NullabilityReadState
    {
        get
        {
            NullabilityState origState = _context.Create(_propertyInfo).ReadState;

            return
                origState == NullabilityState.Unknown || _propertyTypeInGenericDefType is null || !_propertyTypeInGenericDefType.IsGenericTypeParameter
                    ? origState
                    : _declaringType.GetGenericArgumentNullabilityInfo(_propertyTypeInGenericDefType.GenericParameterPosition).State;
        }
    }

    // todo: need to consider nullable related attribute later
    /// <summary>
    /// Gets the nullability write state of current property.
    /// </summary>
    public NullabilityState NullabilityWriteState
    {
        get
        {
            NullabilityState origState = _context.Create(_propertyInfo).WriteState;

            return
                origState == NullabilityState.Unknown || _propertyTypeInGenericDefType is null || !_propertyTypeInGenericDefType.IsGenericTypeParameter
                    ? origState
                    : _declaringType.GetGenericArgumentNullabilityInfo(_propertyTypeInGenericDefType.GenericParameterPosition).State;
        }
    }
}

public partial class NullabilityPropertyInfo : PropertyInfo
{
    public override object[] GetCustomAttributes(bool inherit)
    {
        return _propertyInfo.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _propertyInfo.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return _propertyInfo.IsDefined(attributeType, inherit);
    }

    public override Type? DeclaringType => _propertyInfo.DeclaringType;

    public override string Name => _propertyInfo.Name;

    public override Type? ReflectedType => _propertyInfo.ReflectedType;

    public override MethodInfo[] GetAccessors(bool nonPublic)
    {
        return _propertyInfo.GetAccessors(nonPublic);
    }

    public override MethodInfo? GetGetMethod(bool nonPublic)
    {
        return _propertyInfo.GetGetMethod(nonPublic);
    }

    public override ParameterInfo[] GetIndexParameters()
    {
        return _propertyInfo.GetIndexParameters();
    }

    public override MethodInfo? GetSetMethod(bool nonPublic)
    {
        return _propertyInfo.GetSetMethod(nonPublic);
    }

    public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
    {
        return _propertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
    }

    public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
    {
        _propertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
    }

    public override PropertyAttributes Attributes => _propertyInfo.Attributes;

    public override bool CanRead => _propertyInfo.CanRead;

    public override bool CanWrite => _propertyInfo.CanWrite;

    public override Type PropertyType => _propertyInfo.PropertyType;

    public override bool Equals(object? obj)
    {
        return obj is NullabilityPropertyInfo nullabilityPropertyInfo
        && _propertyInfo.Equals(nullabilityPropertyInfo._propertyInfo)
        && _declaringType.Equals(nullabilityPropertyInfo._declaringType);
    }

    public override object? GetConstantValue()
    {
        return _propertyInfo.GetConstantValue();
    }

    public override int GetHashCode()
    {
        return _propertyInfo.GetHashCode();
    }

    public override Type[] GetOptionalCustomModifiers()
    {
        return _propertyInfo.GetOptionalCustomModifiers();
    }

    public override object? GetRawConstantValue()
    {
        return _propertyInfo.GetRawConstantValue();
    }

    public override Type[] GetRequiredCustomModifiers()
    {
        return _propertyInfo.GetRequiredCustomModifiers();
    }

    public override object? GetValue(object? obj, object?[]? index)
    {
        return _propertyInfo.GetValue(obj, index);
    }

    public override void SetValue(object? obj, object? value, object?[]? index)
    {
        _propertyInfo.SetValue(obj, value, index);
    }

    public override MethodInfo? GetMethod => _propertyInfo.GetMethod;
    public override MemberTypes MemberType => _propertyInfo.MemberType;
    public override MethodInfo? SetMethod => _propertyInfo.SetMethod;
    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return _propertyInfo.GetCustomAttributesData();
    }

    public override bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        return _propertyInfo.HasSameMetadataDefinitionAs(other);
    }

    public override IEnumerable<CustomAttributeData> CustomAttributes => _propertyInfo.CustomAttributes;
    public override bool IsCollectible => _propertyInfo.IsCollectible;
    public override int MetadataToken => _propertyInfo.MetadataToken;
    public override Module Module => _propertyInfo.Module;
    public override string? ToString()
    {
        return _propertyInfo.ToString();
    }
}