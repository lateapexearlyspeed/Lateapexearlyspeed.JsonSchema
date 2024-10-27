using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represent annotated nullability of type, which contains state itself, state of its array element (if it is array) and nested <see cref="NullabilityElement"/> instances of its generic type arguments
/// </summary>
public class NullabilityElement
{
    private readonly NullabilityElement[]? _genericTypeArguments;
    private readonly NullabilityElement? _arrayElement;

    internal NullabilityState State { get; }

    /// <param name="typeInGenericDefType"></param>
    /// <param name="declaringType"></param>
    /// <param name="rawNullabilityInfo">represent the raw nullability info of current type.</param>
    /// <returns></returns>
    internal static NullabilityElement CreateAssembledInfo(Type typeInGenericDefType, NullabilityType declaringType, NullabilityElement rawNullabilityInfo)
    {
        if (typeInGenericDefType.IsGenericTypeParameter)
        {
            NullabilityType genericTypeArg = declaringType.GenericTypeArguments[typeInGenericDefType.GenericParameterPosition];

            return rawNullabilityInfo.State == NullabilityState.Nullable && !genericTypeArg.Type.IsValueType
                ? new NullabilityElement(NullabilityState.Nullable, genericTypeArg.NullabilityInfo._genericTypeArguments, genericTypeArg.NullabilityInfo._arrayElement) 
                : genericTypeArg.NullabilityInfo;
        }

        NullabilityState currentElementState = rawNullabilityInfo.State;

        if (HasArrayElementType(typeInGenericDefType))
        {
            Type? arrayElementType = typeInGenericDefType.GetElementType();

            Debug.Assert(arrayElementType is not null);
            Debug.Assert(rawNullabilityInfo.HasArrayElement);
            NullabilityElement arrayElementInfo = CreateAssembledInfo(arrayElementType, declaringType, rawNullabilityInfo.ArrayElement);

            return new NullabilityElement(currentElementState, null, arrayElementInfo);
        }

        if (typeInGenericDefType.IsGenericType)
        {
            // here 'typeInGenericDefType.IsGenericTypeDefinition' will be true when enclosing generic type contains same generic type (current 'typeInGenericDefType' instance) with same ordered generic type arguments
            Type[] genericTypeArguments = typeInGenericDefType.GenericTypeArgumentsOrParameters();

            NullabilityElement[] genericTypeArgumentsInfo = CreateGenericTypeArgumentElements(genericTypeArguments);

            return new NullabilityElement(currentElementState, genericTypeArgumentsInfo);
        }

        return new NullabilityElement(currentElementState);

        NullabilityElement[] CreateGenericTypeArgumentElements(Type[] genericTypeArguments)
        {
            Debug.Assert(genericTypeArguments.Length == rawNullabilityInfo.GenericTypeArguments.Length);

            var genericTypeArgumentsInfo = new NullabilityElement[genericTypeArguments.Length];

            for (int i = 0; i < genericTypeArguments.Length; i++)
            {
                Type genericTypeArgument = genericTypeArguments[i];
                genericTypeArgumentsInfo[i] = CreateAssembledInfo(genericTypeArgument, declaringType, rawNullabilityInfo.GenericTypeArguments[i]);
            }

            return genericTypeArgumentsInfo;
        }
    }

    private static bool HasArrayElementType(Type type) => type.IsArray && type.HasElementType;

    /// <summary>
    /// Initializes a new instance of the <see cref="NullabilityElement"/> class that contains nullability info of current node and generic type arguments.
    /// </summary>
    /// <param name="state">Annotated <see cref="NullabilityState"/> of current type</param>
    /// <param name="genericTypeArgs">Nullability info of generic type arguments</param>
    public NullabilityElement(NullabilityState state, NullabilityElement[]? genericTypeArgs = null) : this(state, genericTypeArgs, null)
    {
    }

    internal NullabilityElement(NullabilityState state, NullabilityElement[]? genericTypeArgs, NullabilityElement? arrayElement)
    {
        State = state;
        _genericTypeArguments = genericTypeArgs;
        _arrayElement = arrayElement;
    }

    internal NullabilityElement[] GenericTypeArguments => _genericTypeArguments ?? Array.Empty<NullabilityElement>();

    internal NullabilityElement ArrayElement
    {
        get
        {
            if (!HasArrayElement)
            {
                throw new InvalidOperationException("There is no array element info.");
            }

            return _arrayElement;
        }
    }

    [MemberNotNullWhen(true, nameof(_arrayElement))]
    internal bool HasArrayElement => _arrayElement is not null;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not NullabilityElement otherElement
            || State != otherElement.State
            || GenericTypeArguments.Length != otherElement.GenericTypeArguments.Length
            || !Equals(_arrayElement, otherElement._arrayElement))
        {
            return false;
        }

        for (int i = 0; i < GenericTypeArguments.Length; i++)
        {
            if (!GenericTypeArguments[i].Equals(otherElement.GenericTypeArguments[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = _arrayElement is null ? 0 : _arrayElement.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)State;

            foreach (NullabilityElement genericTypeArgument in GenericTypeArguments)
            {
                hashCode = (hashCode * 397) ^ genericTypeArgument.GetHashCode();
            }

            return hashCode;
        }
    }
}