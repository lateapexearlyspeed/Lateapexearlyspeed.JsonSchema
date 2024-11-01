using LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;
using System.Globalization;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="FieldInfo"/> and its annotated nullability info
/// </summary>
public partial class NullabilityFieldInfo
{
    private readonly FieldInfo _fieldInfo;
    private readonly NullabilityType _reflectedType;

    protected internal NullabilityFieldInfo(FieldInfo fieldInfo, NullabilityType reflectedType)
    {
        _fieldInfo = fieldInfo;
        _reflectedType = reflectedType;
    }

    /// <summary>
    /// Gets the nullability state of current field.
    /// </summary>
    public NullabilityState NullabilityState => GetFieldNullabilityInfo().State;

    /// <summary>
    /// Gets the nullability type of current field.
    /// </summary>
    public NullabilityType NullabilityFieldType
    {
        get
        {
            NullabilityElement nullabilityElement = GetFieldNullabilityInfo();

            return new NullabilityType(_fieldInfo.FieldType, nullabilityElement);
        }
    }

    private NullabilityElement GetFieldNullabilityInfo()
    {
        NullabilityType baseClassType = _reflectedType.CreateDeclaringBaseClassType(_fieldInfo.DeclaringType!);

        FieldInfo fieldInfoInDeclaringGenericDefType = baseClassType.Type.GetMemberInfoInGenericDefType(_fieldInfo);

        NullabilityElement fieldRawNullabilityInfo = RawNullabilityAnnotationConverter.ReadField(fieldInfoInDeclaringGenericDefType);

        return NullabilityElement.CreateAssembledInfo(fieldInfoInDeclaringGenericDefType.FieldType, baseClassType, fieldRawNullabilityInfo);
    }
}

public partial class NullabilityFieldInfo : FieldInfo
{
    public override bool Equals(object? obj)
    {
        return obj is NullabilityFieldInfo nullabilityFieldInfo 
               && _fieldInfo.Equals(nullabilityFieldInfo._fieldInfo) 
               && _reflectedType.Equals(nullabilityFieldInfo._reflectedType);
    }

    public override int GetHashCode()
    {
        return _fieldInfo.GetHashCode();
    }

    public override Type[] GetOptionalCustomModifiers()
    {
        return _fieldInfo.GetOptionalCustomModifiers();
    }

    public override object GetRawConstantValue()
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

    public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder? binder, CultureInfo culture)
    {
        _fieldInfo.SetValue(obj, value, invokeAttr, binder, culture);
    }

    public override FieldAttributes Attributes => _fieldInfo.Attributes;

    public override RuntimeFieldHandle FieldHandle => _fieldInfo.FieldHandle;

    public override Type FieldType => _fieldInfo.FieldType;
}