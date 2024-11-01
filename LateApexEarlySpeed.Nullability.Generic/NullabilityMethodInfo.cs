using System.Globalization;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="MethodInfo"/> and annotated nullability info of its parameters and return value
/// </summary>
public partial class NullabilityMethodInfo
{
    private readonly MethodInfo _methodInfo;
    private readonly NullabilityType _reflectedType;

    protected internal NullabilityMethodInfo(MethodInfo method, NullabilityType reflectedType)
    {
        _methodInfo = method;
        _reflectedType = reflectedType;
    }

    /// <summary>
    /// Gets a <see cref="NullabilityParameterInfo"/> object that contains info about the return type of the method, including nullability.
    /// </summary>
    public NullabilityParameterInfo NullabilityReturnParameter
    {
        get
        {
            NullabilityType baseClassType = _reflectedType.CreateDeclaringBaseClassType(_methodInfo.DeclaringType!);

            MethodInfo methodInfoInDeclaringGenericDefType = baseClassType.Type.GetMemberInfoInGenericDefType(_methodInfo);

            return new(_methodInfo.ReturnParameter, methodInfoInDeclaringGenericDefType.ReturnParameter, baseClassType);
        }
    }

    /// <summary>
    /// gets the parameters of the specified method or constructor.
    /// </summary>
    /// <returns>An array of type <see cref="NullabilityParameterInfo"/> containing information (including nullability) that matches the signature of the method (or constructor).</returns>
    public NullabilityParameterInfo[] GetNullabilityParameters()
    {
        NullabilityType baseClassType = _reflectedType.CreateDeclaringBaseClassType(_methodInfo.DeclaringType!);

        MethodInfo methodInfoInDeclaringGenericDefType = baseClassType.Type.GetMemberInfoInGenericDefType(_methodInfo);

        ParameterInfo[] parameters = _methodInfo.GetParameters();
        ParameterInfo[] parametersInGenericDefType = methodInfoInDeclaringGenericDefType.GetParameters();

        return parameters.Select((p, idx) => new NullabilityParameterInfo(p, parametersInGenericDefType[idx], baseClassType)).ToArray();
    }
}

public partial class NullabilityMethodInfo : MethodInfo
{
    public override Delegate CreateDelegate(Type delegateType)
    {
        return _methodInfo.CreateDelegate(delegateType);
    }

    public override Delegate CreateDelegate(Type delegateType, object? target)
    {
        return _methodInfo.CreateDelegate(delegateType, target);
    }

    public override bool Equals(object? obj)
    {
        return obj is NullabilityMethodInfo nullabilityMethodInfo 
               && _methodInfo.Equals(nullabilityMethodInfo._methodInfo)
               && _reflectedType.Equals(nullabilityMethodInfo._reflectedType);
    }

    public override Type[] GetGenericArguments()
    {
        return _methodInfo.GetGenericArguments();
    }

    public override MethodInfo GetGenericMethodDefinition()
    {
        return _methodInfo.GetGenericMethodDefinition();
    }

    public override int GetHashCode()
    {
        return _methodInfo.GetHashCode();
    }

    public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
    {
        return _methodInfo.MakeGenericMethod(typeArguments);
    }

    public override MemberTypes MemberType => _methodInfo.MemberType;
    public override ParameterInfo ReturnParameter => _methodInfo.ReturnParameter;
    public override Type ReturnType => _methodInfo.ReturnType;

    public override MethodBody? GetMethodBody()
    {
        return _methodInfo.GetMethodBody();
    }

    public override ParameterInfo[] GetParameters()
    {
        return _methodInfo.GetParameters();
    }

    public override CallingConventions CallingConvention => _methodInfo.CallingConvention;
    public override bool ContainsGenericParameters => _methodInfo.ContainsGenericParameters;
    public override bool IsConstructedGenericMethod => _methodInfo.IsConstructedGenericMethod;
    public override bool IsGenericMethod => _methodInfo.IsGenericMethod;
    public override bool IsGenericMethodDefinition => _methodInfo.IsGenericMethodDefinition;
    public override bool IsSecurityCritical => _methodInfo.IsSecurityCritical;
    public override bool IsSecuritySafeCritical => _methodInfo.IsSecuritySafeCritical;
    public override bool IsSecurityTransparent => _methodInfo.IsSecurityTransparent;
    public override MethodImplAttributes MethodImplementationFlags => _methodInfo.MethodImplementationFlags;

    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return _methodInfo.GetCustomAttributesData();
    }

    public override bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        return _methodInfo.HasSameMetadataDefinitionAs(other);
    }

    public override IEnumerable<CustomAttributeData> CustomAttributes => _methodInfo.CustomAttributes;
    public override int MetadataToken => _methodInfo.MetadataToken;
    public override Module Module => _methodInfo.Module;

    public override string? ToString()
    {
        return _methodInfo.ToString();
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return _methodInfo.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return _methodInfo.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return _methodInfo.IsDefined(attributeType, inherit);
    }

    public override Type? DeclaringType => _methodInfo.DeclaringType;

    public override string Name => _methodInfo.Name;

    public override Type? ReflectedType => _methodInfo.ReflectedType;

    public override MethodImplAttributes GetMethodImplementationFlags()
    {
        return _methodInfo.GetMethodImplementationFlags();
    }

    public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo culture)
    {
        return _methodInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
    }

    public override MethodAttributes Attributes => _methodInfo.Attributes;

    public override RuntimeMethodHandle MethodHandle => _methodInfo.MethodHandle;

    public override MethodInfo GetBaseDefinition()
    {
        return _methodInfo.GetBaseDefinition();
    }

    public override ICustomAttributeProvider ReturnTypeCustomAttributes => _methodInfo.ReturnTypeCustomAttributes;
}