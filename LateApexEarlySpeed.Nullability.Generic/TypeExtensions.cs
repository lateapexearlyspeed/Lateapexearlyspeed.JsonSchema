using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

internal static class TypeExtensions
{
    public static Type[] GenericTypeArgumentsOrParameters(this Type type)
    {
        return type.IsGenericTypeDefinition ? type.GetTypeInfo().GenericTypeParameters : type.GenericTypeArguments;
    }
}