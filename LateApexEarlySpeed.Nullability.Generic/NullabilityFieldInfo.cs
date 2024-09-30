using System.Globalization;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="FieldInfo"/> and its annotated nullability info
/// </summary>
public partial class NullabilityFieldInfo
{
    private readonly NullabilityInfoContext _context = new();

    private readonly FieldInfo _fieldInfo;
    private readonly Type? _typeInGenericDefType;
    private readonly NullabilityType _declaringType;

    protected internal NullabilityFieldInfo(FieldInfo fieldInfo, Type? typeInGenericDefType, NullabilityType declaringType)
    {
        _fieldInfo = fieldInfo;
        _typeInGenericDefType = typeInGenericDefType;
        _declaringType = declaringType;
    }

    /// <summary>
    /// Gets the nullability state of current field.
    /// </summary>
    public NullabilityState NullabilityState =>
        _typeInGenericDefType is not null && _typeInGenericDefType.IsGenericTypeParameter
            ? _declaringType.GetGenericArgumentNullabilityInfo(_typeInGenericDefType.GenericParameterPosition).State
            : _context.Create(_fieldInfo).ReadState;

    /// <summary>
    /// Gets the nullability type of current field.
    /// </summary>
    public NullabilityType NullabilityAnnotationFieldType
    {
        get
        {
            Type fieldType = _fieldInfo.FieldType;
            NullabilityInfo origNullabilityInfo = _context.Create(_fieldInfo);
            NullabilityElement nullabilityElement = NullabilityElement.Create(_typeInGenericDefType ?? fieldType, _declaringType, origNullabilityInfo);

            return new NullabilityType(fieldType, nullabilityElement);
        }
    }
}

public partial class NullabilityFieldInfo : FieldInfo
{
    public override bool Equals(object? obj)
    {
        return obj is NullabilityFieldInfo nullabilityFieldInfo 
               && _fieldInfo.Equals(nullabilityFieldInfo._fieldInfo) 
               && _declaringType.Equals(nullabilityFieldInfo._declaringType);
    }

    public override int GetHashCode()
    {
        return _fieldInfo.GetHashCode();
    }

    public override Type[] GetOptionalCustomModifiers()
    {
        return _fieldInfo.GetOptionalCustomModifiers();
    }

    public override object? GetRawConstantValue()
    {
        return _fieldInfo.GetRawConstantValue();
    }

    public override Type[] GetRequiredCustomModifiers()
    {
        return _fieldInfo.GetRequiredCustomModifiers();
    }

    public override object? GetValueDirect(TypedReference obj)
    {
        return _fieldInfo.GetValueDirect(obj);
    }

    public override void SetValueDirect(TypedReference obj, object value)
    {
        _fieldInfo.SetValueDirect(obj, value);
    }

    public override bool IsSecurityCritical => _fieldInfo.IsSecurityCritical;
    public override bool IsSecuritySafeCritical => _fieldInfo.IsSecuritySafeCritical;
    public override bool IsSecurityTransparent => _fieldInfo.IsSecurityTransparent;
    public override MemberTypes MemberType => _fieldInfo.MemberType;
    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return _fieldInfo.GetCustomAttributesData();
    }

    public override bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        return _fieldInfo.HasSameMetadataDefinitionAs(other);
    }

    public override IEnumerable<CustomAttributeData> CustomAttributes => _fieldInfo.CustomAttributes;
    public override bool IsCollectible => _fieldInfo.IsCollectible;
    public override int MetadataToken => _fieldInfo.MetadataToken;
    public override Module Module => _fieldInfo.Module;
    public override string? ToString()
    {
        return _fieldInfo.ToString();
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return _fieldInfo.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _fieldInfo.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return _fieldInfo.IsDefined(attributeType, inherit);
    }

    public override Type? DeclaringType => _fieldInfo.DeclaringType;

    public override string Name => _fieldInfo.Name;

    public override Type? ReflectedType => _fieldInfo.ReflectedType;

    public override object? GetValue(object? obj)
    {
        return _fieldInfo.GetValue(obj);
    }

    public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, CultureInfo? culture)
    {
        _fieldInfo.SetValue(obj, value, invokeAttr, binder, culture);
    }

    public override FieldAttributes Attributes => _fieldInfo.Attributes;

    public override RuntimeFieldHandle FieldHandle => _fieldInfo.FieldHandle;

    public override Type FieldType => _fieldInfo.FieldType;
}