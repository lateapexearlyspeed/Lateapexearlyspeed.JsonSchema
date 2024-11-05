using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;

internal static class RawNullabilityAnnotationConverter
{
    private const string CompilerServicesNamespace = "System.Runtime.CompilerServices";
    private const string NullableAttributeName = "NullableAttribute";
    private const string NullableContextAttributeName = "NullableContextAttribute";

    public static NullabilityElement ReadPropertyGetter(PropertyInfo genericDefPropertyInfo)
    {
        return genericDefPropertyInfo.GetGetMethod() is null 
            ? new NullabilityElement(NullabilityState.Unknown) 
            : ReadMemberInfo(genericDefPropertyInfo, genericDefPropertyInfo.PropertyType);
    }

    public static NullabilityElement ReadPropertySetter(PropertyInfo genericDefPropertyInfo)
    {
        return genericDefPropertyInfo.GetSetMethod() is null
            ? new NullabilityElement(NullabilityState.Unknown)
            : ReadMemberInfo(genericDefPropertyInfo, genericDefPropertyInfo.PropertyType);
    }

    public static NullabilityElement ReadField(FieldInfo genericDefFieldInfo)
    {
        return ReadMemberInfo(genericDefFieldInfo, genericDefFieldInfo.FieldType);
    }

    private static NullabilityElement ReadMemberInfo(MemberInfo genericDefMemberInfo, Type memberTypeInGenericDef)
    {
        CustomAttributeData? nullableAttribute = GetNullableAttribute(genericDefMemberInfo);

        if (nullableAttribute is not null)
        {
            return ParseNullableAttribute(memberTypeInGenericDef, nullableAttribute);
        }

        if (TryParseFromType(memberTypeInGenericDef, out NullabilityElement? element))
        {
            return element;
        }

        CustomAttributeData? nullableContextAttribute = FindNullableContextAttributeFor(genericDefMemberInfo);
        if (nullableContextAttribute is null)
        {
            return CreateUnknownElement(memberTypeInGenericDef);
        }

        return ParseNullableContextAttribute(memberTypeInGenericDef, nullableContextAttribute);
    }

    public static NullabilityElement ReadBaseClass(Type type)
    {
        Type? baseType = type.BaseType;
        if (baseType is null || !baseType.IsGenericType)
        {
            return new NullabilityElement(NullabilityState.Unknown);
        }

        CustomAttributeData? nullableAttribute = GetNullableAttribute(type);
        if (nullableAttribute is null)
        {
            return CreateUnknownElement(baseType);
        }

        return ParseNullableAttribute(baseType, nullableAttribute);
    }

    public static NullabilityElement ReadParameter(ParameterInfo genericDefParameterInfo)
    {
        Type parameterTypeInGenericDef = genericDefParameterInfo.ParameterType;
        CustomAttributeData? nullableAttribute = genericDefParameterInfo.GetCustomAttributesData().FirstOrDefault(attr => attr.AttributeType.Namespace == CompilerServicesNamespace && attr.AttributeType.Name == NullableAttributeName);

        if (nullableAttribute is not null)
        {
            return ParseNullableAttribute(parameterTypeInGenericDef, nullableAttribute);
        }

        if (TryParseFromType(parameterTypeInGenericDef, out NullabilityElement? element))
        {
            return element;
        }

        CustomAttributeData? nullableContextAttribute = FindNullableContextAttributeFor(genericDefParameterInfo);
        if (nullableContextAttribute is null)
        {
            return CreateUnknownElement(parameterTypeInGenericDef);
        }

        return ParseNullableContextAttribute(parameterTypeInGenericDef, nullableContextAttribute);

    }

    private static CustomAttributeData? FindNullableContextAttributeFor(ParameterInfo parameterInfo)
    {
        CustomAttributeData? nullableContextAttribute = GetNullableContextAttributeForParameter(parameterInfo);

        MemberInfo? methodOrDeclaringType = parameterInfo.Member;
        while (nullableContextAttribute is null && methodOrDeclaringType is not null)
        {
            nullableContextAttribute = GetNullableContextAttribute(methodOrDeclaringType);
            methodOrDeclaringType = methodOrDeclaringType.DeclaringType;
        }

        return nullableContextAttribute;

        static CustomAttributeData? GetNullableContextAttributeForParameter(ParameterInfo parameterInfo)
        {
            return parameterInfo.GetCustomAttributesData().FirstOrDefault(attr => attr.AttributeType.Namespace == CompilerServicesNamespace && attr.AttributeType.Name == NullableContextAttributeName);
        }
    }

    private static CustomAttributeData? FindNullableContextAttributeFor(MemberInfo memberInfo)
    {
        CustomAttributeData? nullableContextAttribute = GetNullableContextAttribute(memberInfo);

        Type? declaringType = memberInfo.DeclaringType;
        while (nullableContextAttribute is null && declaringType is not null)
        {
            nullableContextAttribute = GetNullableContextAttribute(declaringType);
            declaringType = declaringType.DeclaringType;
        }

        return nullableContextAttribute;
    }

    private static CustomAttributeData? GetNullableAttribute(MemberInfo memberInfo)
    {
        return memberInfo.GetCustomAttributesData().FirstOrDefault(attr => attr.AttributeType.Namespace == CompilerServicesNamespace && attr.AttributeType.Name == NullableAttributeName);
    }

    private static CustomAttributeData? GetNullableContextAttribute(MemberInfo memberInfo)
    {
        return memberInfo.GetCustomAttributesData().FirstOrDefault(attr => attr.AttributeType.Namespace == CompilerServicesNamespace && attr.AttributeType.Name == NullableContextAttributeName);
    }

    private static NullabilityElement ParseNullableContextAttribute(Type type, CustomAttributeData nullableContextAttribute)
    {
        CustomAttributeTypedArgument constructorArg = nullableContextAttribute.ConstructorArguments.Single();
        
        Debug.Assert(constructorArg.ArgumentType == typeof(byte));
        Debug.Assert(constructorArg.Value != null);
        byte b = (byte)constructorArg.Value;

        return ParseNullableAttributeBytes(type, new SingleByteReader(b));
    }

    private static NullabilityElement CreateUnknownElement(Type type)
    {
        if (type.IsGenericTypeParameter)
        {
            return new NullabilityElement(NullabilityState.Unknown);
        }

        Type? underlyingNullableValueType = Nullable.GetUnderlyingType(type);
        if (underlyingNullableValueType is not null)
        {
            return new NullabilityElement(NullabilityState.Nullable, new[] { CreateUnknownElement(underlyingNullableValueType) });
        }

        if (type.IsArray)
        {
            return new NullabilityElement(NullabilityState.Unknown, null, type.HasElementType
                ? CreateUnknownElement(type.GetElementType()!)
                : null);
        }

        NullabilityState currentState = type.IsValueType ? NullabilityState.NotNull : NullabilityState.Unknown;

        return type.IsGenericType 
            ? new NullabilityElement(currentState, type.GenericTypeArgumentsOrParameters().Select(CreateUnknownElement).ToArray()) 
            : new NullabilityElement(currentState);
    }

    private static bool TryParseFromType(Type type, [NotNullWhen(true)] out NullabilityElement? element)
    {
        if (type.IsGenericTypeParameter)
        {
            // T is not constrained to value type
            if ((type.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0)
            {
                element = null;
                return false;
            }

            // T is constrained to value type
            element = new NullabilityElement(NullabilityState.NotNull);
            return true;
        }

        if (!type.IsValueType)
        {
            element = null;
            return false;
        }

        // Below 'type' is value type
        Type? underlyingNullableValueType = Nullable.GetUnderlyingType(type);
        if (underlyingNullableValueType is not null)
        {
            if (TryParseFromType(underlyingNullableValueType, out NullabilityElement? underlyingElement))
            {
                element = new NullabilityElement(NullabilityState.Nullable, new[] { underlyingElement });
                return true;
            }

            element = null;
            return false;
        }

        if (type.IsGenericType)
        {
            Type[] genericTypeArguments = type.GenericTypeArgumentsOrParameters();
            var typeArgElements = new NullabilityElement[genericTypeArguments.Length];
            for (var i = 0; i < genericTypeArguments.Length; i++)
            {
                if (TryParseFromType(genericTypeArguments[i], out NullabilityElement? typeArgElement))
                {
                    typeArgElements[i] = typeArgElement;
                }
                else
                {
                    element = null;
                    return false;
                }
            }

            element = new NullabilityElement(NullabilityState.NotNull, typeArgElements);
            return true;
        }

        // Non-generic value type
        element = new NullabilityElement(NullabilityState.NotNull);
        return true;
    }

    private static NullabilityElement ParseNullableAttribute(Type type, CustomAttributeData nullableAttribute)
    {
        CustomAttributeTypedArgument argument = nullableAttribute.ConstructorArguments.Single();
        Debug.Assert(argument.ArgumentType == typeof(byte) || argument.ArgumentType == typeof(byte[]));

        Debug.Assert(argument.Value is not null);

        if (argument.Value is byte byteValue)
        {
            return ParseNullableAttributeBytes(type, new SingleByteReader(byteValue));
        }

        byte[] bytes = ((ReadOnlyCollection<CustomAttributeTypedArgument>)argument.Value).Select(arg =>
        {
            Debug.Assert(arg.Value is not null);
            return (byte)arg.Value;
        }).ToArray();

        Debug.Assert(bytes.Length > 0);

        var bytesReader = new MultipleBytesReader(bytes);

        NullabilityElement result = ParseNullableAttributeBytes(type, bytesReader);
        Debug.Assert(bytesReader.IsEnd);

        return result;
    }

    private static NullabilityElement ParseNullableAttributeBytes(Type type, IAnnotationBytesReader bytesReader)
    {
        if (type.IsGenericTypeParameter)
        {
            byte b = bytesReader.ReadByte();
            if ((type.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0)
            {
                return MapToElement(b);
            }

            // T is constrained to value type, then its byte value is set to '0'
            Debug.Assert(b == 0);
            return new NullabilityElement(NullabilityState.NotNull);
        }

        if (type.IsValueType && !type.IsGenericType)
        {
            return new NullabilityElement(NullabilityState.NotNull);
        }

        if (type.IsArray)
        {
            return type.HasElementType 
                ? new NullabilityElement(MapToState(bytesReader.ReadByte()), ParseNullableAttributeBytes(type.GetElementType()!, bytesReader)) 
                : new NullabilityElement(MapToState(bytesReader.ReadByte()));
        }

        Type? underlyingNullableValueType = Nullable.GetUnderlyingType(type);
        if (underlyingNullableValueType is not null) // type is nullable value type
        {
            return new NullabilityElement(NullabilityState.Nullable, new[] { ParseNullableAttributeBytes(underlyingNullableValueType, bytesReader) });
        }

        if (type.IsValueType && type.IsGenericType)
        {
            byte b = bytesReader.ReadByte();
            Debug.Assert(b == 0);

            return new NullabilityElement(NullabilityState.NotNull, CreateGenericTypeArgumentsElements(type.GenericTypeArgumentsOrParameters(), bytesReader));
        }

        if (!type.IsValueType)
        {
            return new NullabilityElement(MapToState(bytesReader.ReadByte()), type.IsGenericType
                ? CreateGenericTypeArgumentsElements(type.GenericTypeArgumentsOrParameters(), bytesReader)
                : null);
        }

        throw new Exception($"Issue when parsing type: {type}");

        static NullabilityElement[] CreateGenericTypeArgumentsElements(Type[] genericTypeArguments, IAnnotationBytesReader bytesReader)
        {
            var typeArgumentsElements = new NullabilityElement[genericTypeArguments.Length];
            for (int i = 0; i < genericTypeArguments.Length; i++)
            {
                typeArgumentsElements[i] = ParseNullableAttributeBytes(genericTypeArguments[i], bytesReader);
            }

            return typeArgumentsElements;
        }
    }

    private static NullabilityState MapToState(byte b)
    {
        switch (b)
        {
            case 0:
                return NullabilityState.Unknown;
            case 1:
                return NullabilityState.NotNull;
            case 2:
                return NullabilityState.Nullable;
            default:
                throw new NotSupportedException($"Unexpected byte value: {b}");
        }
    }

    private static NullabilityElement MapToElement(byte b)
    {
        switch (b)
        {
            case 0:
                return new NullabilityElement(NullabilityState.Unknown);
            case 1:
                return new NullabilityElement(NullabilityState.NotNull);
            case 2:
                return new NullabilityElement(NullabilityState.Nullable);
            default:
                throw new NotSupportedException($"Unexpected byte value: {b}");
        }
    }
}