using System.Globalization;
using System.Reflection;
using LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="PropertyInfo"/> and its annotated nullability info
/// </summary>
public partial class NullabilityPropertyInfo
{
    private readonly NullabilityType _reflectedType;

    private volatile NullabilityType? _nullabilityPropertyType;

    /// <summary>
    /// Get the actual runtime PropertyInfo
    /// </summary>
    public PropertyInfo PropertyInfo { get; }

    protected internal NullabilityPropertyInfo(PropertyInfo propertyInfo, NullabilityType reflectedType)
    {
        PropertyInfo = propertyInfo;
        _reflectedType = reflectedType;
    }

    /// <summary>
    /// Gets the nullability type of current property.
    /// </summary>
    public NullabilityType NullabilityPropertyType
    {
        get
        {
            if (_nullabilityPropertyType is null)
            {
                NullabilityElement nullabilityElement = GetPropertyNullabilityInfo();

                _nullabilityPropertyType = new NullabilityType(PropertyInfo.PropertyType, nullabilityElement);
            }

            return _nullabilityPropertyType;
        }
    }

    private NullabilityElement GetPropertyNullabilityInfo()
    {
        NullabilityType baseClassType = _reflectedType.CreateDeclaringBaseClassType(PropertyInfo.DeclaringType!);

        PropertyInfo propertyInfoInDeclaringGenericDefType = baseClassType.Type.GetMemberInfoInGenericDefType(PropertyInfo);

        NullabilityElement propertyRawNullabilityInfo = propertyInfoInDeclaringGenericDefType.GetGetMethod() is null 
            ? RawNullabilityAnnotationConverter.ReadPropertySetter(propertyInfoInDeclaringGenericDefType) 
            : RawNullabilityAnnotationConverter.ReadPropertyGetter(propertyInfoInDeclaringGenericDefType);

        return NullabilityElement.CreateAssembledInfo(propertyInfoInDeclaringGenericDefType.PropertyType, baseClassType, propertyRawNullabilityInfo);
    }

    // todo: need to consider nullable related attribute later
    /// <summary>
    /// Gets the nullability read state of current property.
    /// </summary>
    public NullabilityState NullabilityReadState
    {
        get
        {
            if (PropertyInfo.GetGetMethod() is null)
            {
                return NullabilityState.Unknown;
            }

            return NullabilityPropertyType.NullabilityState;
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
            if (PropertyInfo.GetSetMethod() is null)
            {
                return NullabilityState.Unknown;
            }

            return NullabilityPropertyType.NullabilityState;
        }
    }
}

public partial class NullabilityPropertyInfo : PropertyInfo
{
    public override object[] GetCustomAttributes(bool inherit)
    {
        return PropertyInfo.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return PropertyInfo.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return PropertyInfo.IsDefined(attributeType, inherit);
    }

    public override Type? DeclaringType => PropertyInfo.DeclaringType;

    public override string Name => PropertyInfo.Name;

    public override Type? ReflectedType => PropertyInfo.ReflectedType;

    public override MethodInfo[] GetAccessors(bool nonPublic)
    {
        return PropertyInfo.GetAccessors(nonPublic);
    }

    public override MethodInfo? GetGetMethod(bool nonPublic)
    {
        return PropertyInfo.GetGetMethod(nonPublic);
    }

    public override ParameterInfo[] GetIndexParameters()
    {
        return PropertyInfo.GetIndexParameters();
    }

    public override MethodInfo? GetSetMethod(bool nonPublic)
    {
        return PropertyInfo.GetSetMethod(nonPublic);
    }

    public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
    {
        return PropertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
    }

    public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo culture)
    {
        PropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
    }

    public override PropertyAttributes Attributes => PropertyInfo.Attributes;

    public override bool CanRead => PropertyInfo.CanRead;

    public override bool CanWrite => PropertyInfo.CanWrite;

    public override Type PropertyType => PropertyInfo.PropertyType;

    public override bool Equals(object? obj)
    {
        return obj is NullabilityPropertyInfo nullabilityPropertyInfo
        && PropertyInfo.Equals(nullabilityPropertyInfo.PropertyInfo)
        && _reflectedType.Equals(nullabilityPropertyInfo._reflectedType);
    }

    public override object? GetConstantValue()
    {
        return PropertyInfo.GetConstantValue();
    }

    public override int GetHashCode()
    {
        return PropertyInfo.GetHashCode();
    }

    public override Type[] GetOptionalCustomModifiers()
    {
        return PropertyInfo.GetOptionalCustomModifiers();
    }

    public override object? GetRawConstantValue()
    {
        return PropertyInfo.GetRawConstantValue();
    }

    public override Type[] GetRequiredCustomModifiers()
    {
        return PropertyInfo.GetRequiredCustomModifiers();
    }

    public override object? GetValue(object? obj, object?[]? index)
    {
        return PropertyInfo.GetValue(obj, index);
    }

    public override void SetValue(object? obj, object? value, object?[]? index)
    {
        PropertyInfo.SetValue(obj, value, index);
    }

    public override MethodInfo? GetMethod => PropertyInfo.GetMethod;
    public override MemberTypes MemberType => PropertyInfo.MemberType;
    public override MethodInfo? SetMethod => PropertyInfo.SetMethod;
    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return PropertyInfo.GetCustomAttributesData();
    }

    public override bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        return PropertyInfo.HasSameMetadataDefinitionAs(other);
    }

    public override IEnumerable<CustomAttributeData> CustomAttributes => PropertyInfo.CustomAttributes;
    public override int MetadataToken => PropertyInfo.MetadataToken;
    public override Module Module => PropertyInfo.Module;
    public override string? ToString()
    {
        return PropertyInfo.ToString();
    }
}