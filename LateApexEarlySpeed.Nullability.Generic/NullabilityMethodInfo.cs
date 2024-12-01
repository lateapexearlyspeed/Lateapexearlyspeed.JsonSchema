using System.Globalization;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="MethodInfo"/> and annotated nullability info of its parameters and return value
/// </summary>
public partial class NullabilityMethodInfo
{
    private volatile NullabilityParameterInfo? _nullabilityReturnParameter;
    private volatile NullabilityParameterInfo[]? _nullabilityParameters;

    private readonly NullabilityType _reflectedType;

    /// <summary>
    /// Get the actual runtime MethodInfo
    /// </summary>
    public MethodInfo MethodInfo { get; }

    protected internal NullabilityMethodInfo(MethodInfo method, NullabilityType reflectedType)
    {
        MethodInfo = method;
        _reflectedType = reflectedType;
    }

    /// <summary>
    /// Gets a <see cref="NullabilityParameterInfo"/> object that contains info about the return type of the method, including nullability.
    /// </summary>
    public NullabilityParameterInfo NullabilityReturnParameter
    {
        get
        {
            if (_nullabilityReturnParameter is null)
            {
                NullabilityType baseClassType = _reflectedType.CreateDeclaringBaseClassType(MethodInfo.DeclaringType!);

                MethodInfo methodInfoInDeclaringGenericDefType = baseClassType.Type.GetMemberInfoInGenericDefType(MethodInfo);

                _nullabilityReturnParameter = new(MethodInfo.ReturnParameter, methodInfoInDeclaringGenericDefType.ReturnParameter, baseClassType);
            }

            return _nullabilityReturnParameter;
        }
    }

    /// <summary>
    /// gets the parameters of the specified method or constructor.
    /// </summary>
    /// <returns>An array of type <see cref="NullabilityParameterInfo"/> containing information (including nullability) that matches the signature of the method (or constructor).</returns>
    public NullabilityParameterInfo[] GetNullabilityParameters()
    {
        if (_nullabilityParameters is null)
        {
            NullabilityType baseClassType = _reflectedType.CreateDeclaringBaseClassType(MethodInfo.DeclaringType!);

            MethodInfo methodInfoInDeclaringGenericDefType = baseClassType.Type.GetMemberInfoInGenericDefType(MethodInfo);

            ParameterInfo[] parameters = MethodInfo.GetParameters();
            ParameterInfo[] parametersInGenericDefType = methodInfoInDeclaringGenericDefType.GetParameters();

            _nullabilityParameters = parameters.Select((p, idx) => new NullabilityParameterInfo(p, parametersInGenericDefType[idx], baseClassType)).ToArray();
        }

        return _nullabilityParameters;
    }
}

public partial class NullabilityMethodInfo : MethodInfo
{
    public override Delegate CreateDelegate(Type delegateType)
    {
        return MethodInfo.CreateDelegate(delegateType);
    }

    public override Delegate CreateDelegate(Type delegateType, object? target)
    {
        return MethodInfo.CreateDelegate(delegateType, target);
    }

    public override bool Equals(object? obj)
    {
        return obj is NullabilityMethodInfo nullabilityMethodInfo 
               && MethodInfo.Equals(nullabilityMethodInfo.MethodInfo)
               && _reflectedType.Equals(nullabilityMethodInfo._reflectedType);
    }

    public override Type[] GetGenericArguments()
    {
        return MethodInfo.GetGenericArguments();
    }

    public override MethodInfo GetGenericMethodDefinition()
    {
        return MethodInfo.GetGenericMethodDefinition();
    }

    public override int GetHashCode()
    {
        return MethodInfo.GetHashCode();
    }

    public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
    {
        return MethodInfo.MakeGenericMethod(typeArguments);
    }

    public override MemberTypes MemberType => MethodInfo.MemberType;
    public override ParameterInfo ReturnParameter => MethodInfo.ReturnParameter;
    public override Type ReturnType => MethodInfo.ReturnType;

    public override MethodBody? GetMethodBody()
    {
        return MethodInfo.GetMethodBody();
    }

    public override ParameterInfo[] GetParameters()
    {
        return MethodInfo.GetParameters();
    }

    public override CallingConventions CallingConvention => MethodInfo.CallingConvention;
    public override bool ContainsGenericParameters => MethodInfo.ContainsGenericParameters;
    public override bool IsConstructedGenericMethod => MethodInfo.IsConstructedGenericMethod;
    public override bool IsGenericMethod => MethodInfo.IsGenericMethod;
    public override bool IsGenericMethodDefinition => MethodInfo.IsGenericMethodDefinition;
    public override bool IsSecurityCritical => MethodInfo.IsSecurityCritical;
    public override bool IsSecuritySafeCritical => MethodInfo.IsSecuritySafeCritical;
    public override bool IsSecurityTransparent => MethodInfo.IsSecurityTransparent;
    public override MethodImplAttributes MethodImplementationFlags => MethodInfo.MethodImplementationFlags;

    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return MethodInfo.GetCustomAttributesData();
    }

    public override bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        return MethodInfo.HasSameMetadataDefinitionAs(other);
    }

    public override IEnumerable<CustomAttributeData> CustomAttributes => MethodInfo.CustomAttributes;
    public override int MetadataToken => MethodInfo.MetadataToken;
    public override Module Module => MethodInfo.Module;

    public override string? ToString()
    {
        return MethodInfo.ToString();
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return MethodInfo.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return MethodInfo.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return MethodInfo.IsDefined(attributeType, inherit);
    }

    public override Type? DeclaringType => MethodInfo.DeclaringType;

    public override string Name => MethodInfo.Name;

    public override Type? ReflectedType => MethodInfo.ReflectedType;

    public override MethodImplAttributes GetMethodImplementationFlags()
    {
        return MethodInfo.GetMethodImplementationFlags();
    }

    public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo culture)
    {
        return MethodInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
    }

    public override MethodAttributes Attributes => MethodInfo.Attributes;

    public override RuntimeMethodHandle MethodHandle => MethodInfo.MethodHandle;

    public override MethodInfo GetBaseDefinition()
    {
        return MethodInfo.GetBaseDefinition();
    }

    public override ICustomAttributeProvider ReturnTypeCustomAttributes => MethodInfo.ReturnTypeCustomAttributes;
}