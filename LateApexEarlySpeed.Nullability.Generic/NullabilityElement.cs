using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represent annotated nullability of type, which contains state itself, state of its array element (if it is array) and nested <see cref="NullabilityElement"/> instances of its generic type arguments
/// </summary>
public class NullabilityElement
{
    private static readonly INullabilityStatePolicy DefaultNullabilityStatePolicy = new NullabilityStatePolicy();

    private readonly NullabilityElement[]? _genericTypeArguments;
    private readonly NullabilityElement? _arrayElement;

    internal NullabilityState State { get; }

    /// <param name="typeInGenericDefType"></param>
    /// <param name="declaringType"></param>
    /// <param name="nullabilityInfo">represent the nullability info of current type if <paramref name="calledFromNullableValueType"/> is false;
    /// Otherwise represent nullability info of <see cref="Nullable{T}"/> itself. </param>
    /// <param name="calledFromNullableValueType">Specify whether current <paramref name="typeInGenericDefType"/> is underlying type of <see cref="Nullable{T}"/> </param>
    /// <param name="nullabilityStatePolicy"></param>
    /// <returns></returns>
    internal static NullabilityElement Create(Type typeInGenericDefType, NullabilityType declaringType, NullabilityInfo nullabilityInfo, bool calledFromNullableValueType = false, INullabilityStatePolicy? nullabilityStatePolicy = null)
    {
        nullabilityStatePolicy ??= DefaultNullabilityStatePolicy;

        if (typeInGenericDefType.IsGenericTypeParameter)
        {
            return declaringType.GetGenericArgumentNullabilityInfo(typeInGenericDefType.GenericParameterPosition);
        }

        NullabilityState currentElementState = calledFromNullableValueType 
            ? NullabilityState.NotNull
            : nullabilityStatePolicy.FindState(nullabilityInfo);

        if (HasArrayElementType(typeInGenericDefType))
        {
            Type? arrayElementType = typeInGenericDefType.GetElementType();

            Debug.Assert(arrayElementType is not null);
            Debug.Assert(nullabilityInfo.ElementType is not null);
            NullabilityElement arrayElementInfo = Create(arrayElementType, declaringType, nullabilityInfo.ElementType, false, nullabilityStatePolicy);

            return new NullabilityElement(currentElementState, null, arrayElementInfo);
        }

        if (typeInGenericDefType.IsGenericType)
        {
            Type? underlyingTypeOfNullableValue = Nullable.GetUnderlyingType(typeInGenericDefType);
            if (underlyingTypeOfNullableValue is null)
            {
                Type[] genericTypeArguments = typeInGenericDefType.IsGenericTypeDefinition // here will be true when enclosing generic type contains same generic type (current 'typeInGenericDefType' instance) with same ordered generic type arguments
                ? typeInGenericDefType.GetTypeInfo().GenericTypeParameters 
                : typeInGenericDefType.GenericTypeArguments;

                NullabilityElement[] genericTypeArgumentsInfo = CreateGenericTypeArgumentElements(genericTypeArguments);

                return new NullabilityElement(currentElementState, genericTypeArgumentsInfo);
            }
            else // current type is nullable value type
            {
                NullabilityElement underlyingTypeElement = Create(underlyingTypeOfNullableValue, declaringType, nullabilityInfo, true, nullabilityStatePolicy);
                return new NullabilityElement(currentElementState, new[] { underlyingTypeElement });
            }
        }

        return new NullabilityElement(currentElementState);

        NullabilityElement[] CreateGenericTypeArgumentElements(Type[] genericTypeArguments)
        {
            Debug.Assert(genericTypeArguments.Length == nullabilityInfo.GenericTypeArguments.Length);

            var genericTypeArgumentsInfo = new NullabilityElement[genericTypeArguments.Length];

            for (int i = 0; i < genericTypeArguments.Length; i++)
            {
                Type genericTypeArgument = genericTypeArguments[i];
                genericTypeArgumentsInfo[i] = Create(genericTypeArgument, declaringType, nullabilityInfo.GenericTypeArguments[i], false, nullabilityStatePolicy);
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