using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class TypeWrapper : IType
{
    public TypeWrapper(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
    public IType[] GenericTypeArguments => Type.GenericTypeArguments.Select(arg => new TypeWrapper(arg)).ToArray<IType>();
    public IType GetArrayElementType()
    {
        if (!Type.IsArray || !Type.HasElementType)
        {
            throw new InvalidOperationException($"Current type: {Type.FullName} is not an array type with element");
        }

        return new TypeWrapper(Type.GetElementType()!);
    }

    public IPropertyInfo[] GetProperties(BindingFlags bindingAttr)
    {
        PropertyInfo[] propertyInfos = Type.GetProperties(bindingAttr);

        return propertyInfos.Select(prop => new PropertyInfoWrapper(prop)).ToArray<IPropertyInfo>();
    }

    public IFieldInfo[] GetFields(BindingFlags bindingAttr)
    {
        FieldInfo[] fields = Type.GetFields(bindingAttr);

        return fields.Select(f => new FieldInfoWrapper(f)).ToArray<IFieldInfo>();
    }

    public IMethodInfo[] GetMethods(BindingFlags bindingAttr)
    {
        MethodInfo[] methodInfos = Type.GetMethods(bindingAttr);

        return methodInfos.Select(m => new MethodInfoWrapper(m)).ToArray<IMethodInfo>();
    }
}