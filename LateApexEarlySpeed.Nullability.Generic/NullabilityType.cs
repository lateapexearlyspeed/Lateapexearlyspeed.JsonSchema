using System.Collections.Concurrent;
using LateApexEarlySpeed.Nullability.Generic.RawNullabilityAnnotation;
using System.Diagnostics;
using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

/// <summary>
/// Represents standard <see cref="Type"/> and its annotated nullability
/// </summary>
public class NullabilityType
{
    private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

    private readonly ConcurrentDictionary<PropertyInfo, NullabilityPropertyInfo> _propertiesCache = new();
    private readonly ConcurrentDictionary<FieldInfo, NullabilityFieldInfo> _fieldsCache = new();
    private readonly ConcurrentDictionary<MethodInfo, NullabilityMethodInfo> _methodsCache = new();

    internal readonly NullabilityElement NullabilityInfo;

    /// <summary>
    /// Get the actual runtime type
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Get the annotated nullability state of current type
    /// </summary>
    public NullabilityState NullabilityState => NullabilityInfo.State;

    internal NullabilityType(Type type, NullabilityElement nullabilityInfo)
    {
        NullabilityInfo = nullabilityInfo;
        Type = type;
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
        => Type.GenericTypeArguments.Select((type, idx) => new NullabilityType(type, NullabilityInfo.GenericTypeArguments[idx])).ToArray();

    /// <summary>
    /// returns the <see cref="NullabilityType"/> of the elements in current array, or null if the current Type is not an array
    /// </summary>
    /// <returns></returns>
    public NullabilityType? GetArrayElementType()
    {
        if (!NullabilityInfo.HasArrayElement)
        {
            return null;
        }

        NullabilityElement arrayElement = NullabilityInfo.ArrayElement;

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

        return GetOrAddToCache(propertyInfo);
    }

    public NullabilityPropertyInfo? GetProperty(string name, Type returnType)
    {
        PropertyInfo? propertyInfo = Type.GetProperty(name, returnType);
        if (propertyInfo is null)
        {
            return null;
        }

        return GetOrAddToCache(propertyInfo);
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

        return GetOrAddToCache(propertyInfo);
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

        return propertyInfos.Select(GetOrAddToCache).ToArray();
    }

    private NullabilityPropertyInfo GetOrAddToCache(PropertyInfo propertyInfo)
    {
        return _propertiesCache.GetOrAdd(propertyInfo, static (prop, reflectedType) => new NullabilityPropertyInfo(prop, reflectedType), this);
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

        return GetOrAddToCache(fieldInfo);
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

        return fieldInfos.Select(GetOrAddToCache).ToArray();
    }

    private NullabilityFieldInfo GetOrAddToCache(FieldInfo fieldInfo)
    {
        return _fieldsCache.GetOrAdd(fieldInfo, static (field, reflectedType) => new NullabilityFieldInfo(field, reflectedType), this);
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

        return GetOrAddToCache(method);
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

        return GetOrAddToCache(method);
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

        return GetOrAddToCache(method);
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

        return methodInfos.Select(GetOrAddToCache).ToArray();
    }

    private NullabilityMethodInfo GetOrAddToCache(MethodInfo methodInfo)
    {
        return _methodsCache.GetOrAdd(methodInfo, static (method, reflectedType) => new NullabilityMethodInfo(method, reflectedType), this);
    }

    internal NullabilityType CreateDeclaringBaseClassType(Type declaringType)
    {
        Debug.Assert((Type.IsInterface && Type == declaringType) || (!Type.IsInterface && !declaringType.IsInterface));

        NullabilityType baseClassType = this;

        while (baseClassType.Type != declaringType)
        {
            Type subClass = baseClassType.Type;
            Type subClassGenericDef = subClass.GetGenericTypeDefinitionIfIsGenericType();
            NullabilityElement baseClassRawNullabilityInfo = RawNullabilityAnnotationConverter.ReadBaseClass(subClassGenericDef);
            Debug.Assert(subClassGenericDef.BaseType is not null);
            NullabilityElement baseClassNullabilityInfo = NullabilityElement.CreateAssembledInfo(subClassGenericDef.BaseType, baseClassType, baseClassRawNullabilityInfo);
            Debug.Assert(subClass.BaseType is not null);
            baseClassType = new NullabilityType(subClass.BaseType, baseClassNullabilityInfo);
        }

        return baseClassType;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is NullabilityType otherNullabilityType 
        && Type == otherNullabilityType.Type 
        && NullabilityInfo.Equals(otherNullabilityType.NullabilityInfo);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }
}