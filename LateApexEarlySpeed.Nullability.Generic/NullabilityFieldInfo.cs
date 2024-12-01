using LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;
using System.Globalization;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="FieldInfo"/> and its annotated nullability info
/// </summary>
public partial class NullabilityFieldInfo
{
    private readonly NullabilityType _reflectedType;

    private volatile NullabilityType? _nullabilityFieldType;

    /// <summary>
    /// Get the actual runtime FieldInfo
    /// </summary>
    public FieldInfo FieldInfo { get; }

    protected internal NullabilityFieldInfo(FieldInfo fieldInfo, NullabilityType reflectedType)
    {
        FieldInfo = fieldInfo;
        _reflectedType = reflectedType;
    }

    /// <summary>
    /// Gets the nullability state of current field.
    /// </summary>
    public NullabilityState NullabilityState => NullabilityFieldType.NullabilityState;

    /// <summary>
    /// Gets the nullability type of current field.
    /// </summary>
    public NullabilityType NullabilityFieldType
    {
        get
        {
            if (_nullabilityFieldType is null)
            {
                NullabilityElement nullabilityElement = GetFieldNullabilityInfo();

                _nullabilityFieldType = new NullabilityType(FieldInfo.FieldType, nullabilityElement);
            }

            return _nullabilityFieldType;
        }
    }

    private NullabilityElement GetFieldNullabilityInfo()
    {
        NullabilityType baseClassType = _reflectedType.CreateDeclaringBaseClassType(FieldInfo.DeclaringType!);

        FieldInfo fieldInfoInDeclaringGenericDefType = baseClassType.Type.GetMemberInfoInGenericDefType(FieldInfo);

        NullabilityElement fieldRawNullabilityInfo = RawNullabilityAnnotationConverter.ReadField(fieldInfoInDeclaringGenericDefType);

        return NullabilityElement.CreateAssembledInfo(fieldInfoInDeclaringGenericDefType.FieldType, baseClassType, fieldRawNullabilityInfo);
    }
}

public partial class NullabilityFieldInfo : FieldInfo
{
    public override bool Equals(object? obj)
    {
        return obj is NullabilityFieldInfo nullabilityFieldInfo 
               && FieldInfo.Equals(nullabilityFieldInfo.FieldInfo) 
               && _reflectedType.Equals(nullabilityFieldInfo._reflectedType);
    }

    public override int GetHashCode()
    {
        return FieldInfo.GetHashCode();
    }

    public override Type[] GetOptionalCustomModifiers()
    {
        return FieldInfo.GetOptionalCustomModifiers();
    }

    public override object GetRawConstantValue()
    {
        return FieldInfo.GetRawConstantValue();
    }

    public override Type[] GetRequiredCustomModifiers()
    {
        return FieldInfo.GetRequiredCustomModifiers();
    }

    public override object? GetValueDirect(TypedReference obj)
    {
        return FieldInfo.GetValueDirect(obj);
    }

    public override void SetValueDirect(TypedReference obj, object value)
    {
        FieldInfo.SetValueDirect(obj, value);
    }

    public override bool IsSecurityCritical => FieldInfo.IsSecurityCritical;
    public override bool IsSecuritySafeCritical => FieldInfo.IsSecuritySafeCritical;
    public override bool IsSecurityTransparent => FieldInfo.IsSecurityTransparent;
    public override MemberTypes MemberType => FieldInfo.MemberType;
    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return FieldInfo.GetCustomAttributesData();
    }

    public override bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        return FieldInfo.HasSameMetadataDefinitionAs(other);
    }

    public override IEnumerable<CustomAttributeData> CustomAttributes => FieldInfo.CustomAttributes;
    public override int MetadataToken => FieldInfo.MetadataToken;
    public override Module Module => FieldInfo.Module;
    public override string? ToString()
    {
        return FieldInfo.ToString();
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return FieldInfo.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return FieldInfo.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return FieldInfo.IsDefined(attributeType, inherit);
    }

    public override Type? DeclaringType => FieldInfo.DeclaringType;

    public override string Name => FieldInfo.Name;

    public override Type? ReflectedType => FieldInfo.ReflectedType;

    public override object? GetValue(object? obj)
    {
        return FieldInfo.GetValue(obj);
    }

    public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder? binder, CultureInfo culture)
    {
        FieldInfo.SetValue(obj, value, invokeAttr, binder, culture);
    }

    public override FieldAttributes Attributes => FieldInfo.Attributes;

    public override RuntimeFieldHandle FieldHandle => FieldInfo.FieldHandle;

    public override Type FieldType => FieldInfo.FieldType;
}