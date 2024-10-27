using System.Reflection;
using LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="ParameterInfo"/> and its annotated nullability info
/// </summary>
public partial class NullabilityParameterInfo
{
    private readonly ParameterInfo _parameterInfo;
    private readonly ParameterInfo _parameterInDeclaringGenericDefType;
    private readonly NullabilityType _declaringType;

    public NullabilityParameterInfo(ParameterInfo parameter, ParameterInfo parameterInDeclaringGenericDefType, NullabilityType declaringType)
    {
        _parameterInfo = parameter;
        _parameterInDeclaringGenericDefType = parameterInDeclaringGenericDefType;
        _declaringType = declaringType;
    }

    /// <summary>
    /// Gets the nullability state of current parameter.
    /// </summary>
    public NullabilityState NullabilityState => GetParameterNullabilityInfo().State;

    /// <summary>
    /// Gets the nullability type of current parameter.
    /// </summary>
    public NullabilityType NullabilityParameterType
    {
        get
        {
            NullabilityElement nullabilityElement = GetParameterNullabilityInfo();

            return new NullabilityType(_parameterInfo.ParameterType, nullabilityElement);
        }
    }

    private NullabilityElement GetParameterNullabilityInfo()
    {
        NullabilityElement rawParameterInfo = RawNullabilityAnnotationConverter.ReadParameter(_parameterInDeclaringGenericDefType);
        return NullabilityElement.CreateAssembledInfo(_parameterInDeclaringGenericDefType.ParameterType, _declaringType, rawParameterInfo);
    }
}

public partial class NullabilityParameterInfo : ParameterInfo
{
    public override object[] GetCustomAttributes(bool inherit)
    {
        return _parameterInfo.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _parameterInfo.GetCustomAttributes(attributeType, inherit);
    }

    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return _parameterInfo.GetCustomAttributesData();
    }

    public override Type[] GetOptionalCustomModifiers()
    {
        return _parameterInfo.GetOptionalCustomModifiers();
    }

    public override Type[] GetRequiredCustomModifiers()
    {
        return _parameterInfo.GetRequiredCustomModifiers();
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return _parameterInfo.IsDefined(attributeType, inherit);
    }

    public override string ToString()
    {
        return _parameterInfo.ToString();
    }

    public override ParameterAttributes Attributes => _parameterInfo.Attributes;
    public override IEnumerable<CustomAttributeData> CustomAttributes => _parameterInfo.CustomAttributes;
    public override object? DefaultValue => _parameterInfo.DefaultValue;
    public override bool HasDefaultValue => _parameterInfo.HasDefaultValue;
    public override MemberInfo Member => _parameterInfo.Member;
    public override int MetadataToken => _parameterInfo.MetadataToken;
    public override string? Name => _parameterInfo.Name;
    public override Type ParameterType => _parameterInfo.ParameterType;
    public override int Position => _parameterInfo.Position;
    public override object? RawDefaultValue => _parameterInfo.RawDefaultValue;

    public override bool Equals(object? obj)
    {
        return obj is NullabilityParameterInfo nullabilityParameterInfo
        && _parameterInfo.Equals(nullabilityParameterInfo._parameterInfo)
        && _declaringType.Equals(nullabilityParameterInfo._declaringType);
    }

    public override int GetHashCode()
    {
        return _parameterInfo.GetHashCode();
    }
}