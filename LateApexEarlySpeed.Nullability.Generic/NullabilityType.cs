using System.Diagnostics;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="Type"/> and its annotated nullability
/// </summary>
public class NullabilityType
{
    private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

    private readonly NullabilityElement _nullabilityInfo;
    private readonly Type? _genericDefType;

    /// <summary>
    /// Get the actual runtime type
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Get the annotated nullability state of current type
    /// </summary>
    public NullabilityState NullabilityState => _nullabilityInfo.State;

    internal NullabilityType(Type type, NullabilityElement nullabilityInfo)
    {
        _nullabilityInfo = nullabilityInfo;
        Type = type;
        if (type.IsGenericType)
        {
            _genericDefType = type.GetGenericTypeDefinition();
        }
    }

    /// <summary>
    /// Initialize new instance of <see cref="NullabilityType"/> from actual runtime type instance
    /// </summary>
    /// <param name="type">actual runtime type instance </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"><paramref name="type"/> is generic type </exception>
    /// <remarks>Use this construct when <paramref name="type"/> itself is not generic type</remarks>
    public static NullabilityType GetType(Type type)
    {
        if (type.IsGenericType)
        {
            throw new ArgumentException($"type should be non-generic one, or use other {nameof(GetType)} overloads", nameof(type));
        }

        return new NullabilityType(type, new NullabilityElement(GetRootTypeNullabilityState(type)));
    }

    /// <summary>
    /// Initialize new instance of <see cref="NullabilityType"/> from actual runtime type instance with specified nullability info of generic type arguments
    /// </summary>
    /// <param name="type">actual runtime type instance </param>
    /// <param name="genericTypeArgumentsNullabilities">nullability state of generic type arguments</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">generic type argument info of <paramref name="type"/> not match with <paramref name="genericTypeArgumentsNullabilities"/>></exception>
    /// <remarks>Use this construct when <paramref name="type"/> is generic type and its generic type arguments are not generic types</remarks>
    public static NullabilityType GetType(Type type, params NullabilityState[] genericTypeArgumentsNullabilities)
    {
        return GetType(type, genericTypeArgumentsNullabilities.Select(arg => new NullabilityElement(arg)).ToArray());
    }

    /// <summary>
    /// Initialize new instance of <see cref="NullabilityType"/> from actual runtime type instance with specified nullability info of generic type arguments
    /// </summary>
    /// <param name="type">actual runtime type instance </param>
    /// <param name="genericTypeArgumentsNullabilities">nullability info of generic type arguments and their child generic type arguments</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">generic type argument info of <paramref name="type"/> not match with <paramref name="genericTypeArgumentsNullabilities"/>></exception>
    /// <remarks>Use this construct when <paramref name="type"/> is generic type and its generic type arguments are also generic type</remarks>
    public static NullabilityType GetType(Type type, params NullabilityElement[] genericTypeArgumentsNullabilities)
    {
        if (type.IsGenericTypeDefinition)
        {
            throw new NotSupportedException($"{nameof(type)} is generic type definition which is not supported currently. Please raise requirement issue if need.");
        }

        ThrowIfGenericTypeArgumentsNotMatch(type, genericTypeArgumentsNullabilities);

        return new NullabilityType(type, new NullabilityElement(GetRootTypeNullabilityState(type), genericTypeArgumentsNullabilities));
    }

    private static NullabilityState GetRootTypeNullabilityState(Type type)
    {
        if (type.IsValueType)
        {
            return Nullable.GetUnderlyingType(type) is null
                ? NullabilityState.NotNull
                : NullabilityState.Nullable;
        }

        return NullabilityState.Unknown;
    }

    private static void ThrowIfGenericTypeArgumentsNotMatch(Type type, NullabilityElement[] genericTypeArgNullabilities)
    {
        Type[] typeArguments = type.GenericTypeArguments;
        if (typeArguments.Length != genericTypeArgNullabilities.Length)
        {
            throw new ArgumentException("Generic type arguments count mismatch");
        }

        for (int i = 0; i < typeArguments.Length; i++)
        {
            ThrowIfGenericTypeArgumentsNotMatch(type.GenericTypeArguments[i], genericTypeArgNullabilities[i].GenericTypeArguments);
        }

    }

    /// <summary>
    /// Gets an array of the generic type arguments for this type.
    /// </summary>
    public NullabilityType[] GenericTypeArguments
        => Type.GenericTypeArguments.Select((type, idx) => new NullabilityType(type, _nullabilityInfo.GenericTypeArguments[idx])).ToArray();

    /// <summary>
    /// returns the <see cref="NullabilityType"/> of the elements in current array, or null if the current Type is not an array
    /// </summary>
    /// <returns></returns>
    public NullabilityType? GetArrayElementType()
    {
        if (!_nullabilityInfo.HasArrayElement)
        {
            return null;
        }

        NullabilityElement arrayElement = _nullabilityInfo.ArrayElement;

        Type? arrayElementType = Type.GetElementType();
        Debug.Assert(arrayElementType is not null);

        return new NullabilityType(arrayElementType, arrayElement);
    }

    public NullabilityPropertyInfo? GetProperty(string name)
    {
        return GetProperty(name, DefaultLookup);
    }

    public NullabilityPropertyInfo? GetProperty(string name, BindingFlags bindingAttr)
    {
        PropertyInfo? propertyInfo = Type.GetProperty(name, bindingAttr);
        if (propertyInfo is null)
        {
            return null;
        }

        Type? genericDefPropertyType = _genericDefType?.GetProperty(name, bindingAttr)!.PropertyType;

        return new NullabilityPropertyInfo(propertyInfo, genericDefPropertyType, this);
    }

    public NullabilityPropertyInfo? GetProperty(string name, Type? returnType)
    {
        PropertyInfo? propertyInfo = Type.GetProperty(name, returnType);
        if (propertyInfo is null)
        {
            return null;
        }

        Type? genericDefPropertyType = _genericDefType?.GetProperty(name, returnType)!.PropertyType;

        return new NullabilityPropertyInfo(propertyInfo, genericDefPropertyType, this);
    }

    public NullabilityPropertyInfo? GetProperty(string name, Type? returnType, Type[] types)
    {
        return GetProperty(name, returnType, types, null);
    }

    public NullabilityPropertyInfo? GetProperty(string name, Type? returnType, Type[] types, ParameterModifier[]? modifiers)
    {
        return GetProperty(name, DefaultLookup, null, returnType, types, modifiers);
    }

    public NullabilityPropertyInfo? GetProperty(string name, Type[] types)
    {
        return GetProperty(name, null, types);
    }

    public NullabilityPropertyInfo? GetProperty(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[] types, ParameterModifier[]? modifiers)
    {
        PropertyInfo? propertyInfo = Type.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        if (propertyInfo is null)
        {
            return null;
        }

        Type? genericDefPropertyType = _genericDefType?.GetProperty(name, bindingAttr, binder, returnType, types, modifiers)!.PropertyType;

        return new NullabilityPropertyInfo(propertyInfo, genericDefPropertyType, this);
    }

    public NullabilityPropertyInfo[] GetProperties()
    {
        return GetProperties(DefaultLookup);
    }

    public NullabilityPropertyInfo[] GetProperties(BindingFlags bindingAttr)
    {
        PropertyInfo[] propertyInfos = Type.GetProperties(bindingAttr);

        if (propertyInfos.Length == 0)
        {
            return Array.Empty<NullabilityPropertyInfo>();
        }

        PropertyInfo[]? genericDefTypeProperties = _genericDefType?.GetProperties(bindingAttr);

        return propertyInfos.Select(p => new NullabilityPropertyInfo(p, genericDefTypeProperties?.First(genericProp => genericProp.HasSameMetadataDefinitionAs(p)).PropertyType, this)).ToArray();
    }

    public NullabilityFieldInfo? GetField(string name)
    {
        return GetField(name, DefaultLookup);
    }

    public NullabilityFieldInfo? GetField(string name, BindingFlags bindingAttr)
    {
        FieldInfo? fieldInfo = Type.GetField(name, bindingAttr);
        if (fieldInfo is null)
        {
            return null;
        }

        Type? genericDefFieldType = _genericDefType?.GetField(name, bindingAttr)!.FieldType;

        return new NullabilityFieldInfo(fieldInfo, genericDefFieldType, this);
    }

    public NullabilityFieldInfo[] GetFields()
    {
        return GetFields(DefaultLookup);
    }

    public NullabilityFieldInfo[] GetFields(BindingFlags bindingAttr)
    {
        FieldInfo[] fieldInfos = Type.GetFields(bindingAttr);
        if (fieldInfos.Length == 0)
        {
            return Array.Empty<NullabilityFieldInfo>();
        }

        FieldInfo[]? fieldsInGenericDefType = _genericDefType?.GetFields(bindingAttr);

        return fieldInfos.Select(f => new NullabilityFieldInfo(f, fieldsInGenericDefType?.First(genericField => genericField.HasSameMetadataDefinitionAs(f)).FieldType, this)).ToArray();
    }

    public NullabilityMethodInfo? GetMethod(string name)
    {
        return GetMethod(name, DefaultLookup);
    }

    public NullabilityMethodInfo? GetMethod(string name, BindingFlags bindingAttr)
    {
        MethodInfo? method = Type.GetMethod(name, bindingAttr);
        if (method is null)
        {
            return null;
        }

        MethodInfo? genericDefMethod = _genericDefType?.GetMethod(name, bindingAttr);

        return new NullabilityMethodInfo(method, genericDefMethod, this);
    }

    public NullabilityMethodInfo? GetMethod(string name, BindingFlags bindingAttr, Binder? binder, Type[] types, ParameterModifier[]? modifiers)
    {
        return GetMethod(name, bindingAttr, binder, CallingConventions.Any, types, modifiers);
    }

    public NullabilityMethodInfo? GetMethod(string name, BindingFlags bindingAttr, Type[] types)
    {
        return GetMethod(name, bindingAttr, binder: null, types, modifiers: null);
    }

    public NullabilityMethodInfo? GetMethod(string name, Type[] types)
    {
        return GetMethod(name, types, null);
    }

    public NullabilityMethodInfo? GetMethod(string name, Type[] types, ParameterModifier[]? modifiers)
    {
        return GetMethod(name, DefaultLookup, null, types, modifiers);
    }

    public NullabilityMethodInfo? GetMethod(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers)
    {
        MethodInfo? method = Type.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        if (method is null)
        {
            return null;
        }

        MethodInfo? genericDefMethod = _genericDefType?.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);

        return new NullabilityMethodInfo(method, genericDefMethod, this);
    }

    public NullabilityMethodInfo? GetMethod(string name, int genericParameterCount, Type[] types)
    {
        return GetMethod(name, genericParameterCount, types, null);
    }

    public NullabilityMethodInfo? GetMethod(string name, int genericParameterCount, Type[] types, ParameterModifier[]? modifiers)
    {
        return GetMethod(name, genericParameterCount, DefaultLookup, null, CallingConventions.Any, types, modifiers);
    }

    public NullabilityMethodInfo? GetMethod(string name, int genericParameterCount, BindingFlags bindingAttr, Binder? binder, Type[] types, ParameterModifier[]? modifiers)
    {
        return GetMethod(name, genericParameterCount, bindingAttr, binder, CallingConventions.Any, types, modifiers);
    }

    public NullabilityMethodInfo? GetMethod(string name, int genericParameterCount, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers)
    {
        MethodInfo? method = Type.GetMethod(name, genericParameterCount, bindingAttr, binder, callConvention, types, modifiers);
        if (method is null)
        {
            return null;
        }

        MethodInfo? genericDefMethod = _genericDefType?.GetMethod(name, genericParameterCount, bindingAttr, binder, callConvention, types, modifiers);

        return new NullabilityMethodInfo(method, genericDefMethod, this);
    }

    public NullabilityMethodInfo[] GetMethods()
    {
        return GetMethods(DefaultLookup);
    }

    public NullabilityMethodInfo[] GetMethods(BindingFlags bindingAttr)
    {
        MethodInfo[] methodInfos = Type.GetMethods(bindingAttr);
        if (methodInfos.Length == 0)
        {
            return Array.Empty<NullabilityMethodInfo>();
        }

        MethodInfo[]? methodsInGenericDefType = _genericDefType?.GetMethods(bindingAttr);

        return methodInfos.Select(m => new NullabilityMethodInfo(m, methodsInGenericDefType?.First(genericMethod => genericMethod.HasSameMetadataDefinitionAs(m)), this)).ToArray();
    }

    internal NullabilityElement GetGenericArgumentNullabilityInfo(int argumentIdx)
    {
        return _nullabilityInfo.GenericTypeArguments[argumentIdx];
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is NullabilityType otherNullabilityType 
        && Type == otherNullabilityType.Type 
        && _nullabilityInfo.Equals(otherNullabilityType._nullabilityInfo);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }
}