using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal interface IType
{
    Type Type { get; }

    IType[] GenericTypeArguments { get; }
    IType GetArrayElementType();

    IPropertyInfo[] GetProperties(BindingFlags bindingAttr);
    IFieldInfo[] GetFields(BindingFlags bindingAttr);
    IMethodInfo[] GetMethods(BindingFlags bindingAttr);
}