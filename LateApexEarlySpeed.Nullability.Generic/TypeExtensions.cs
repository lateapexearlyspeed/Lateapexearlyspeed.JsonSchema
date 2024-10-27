using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

internal static class TypeExtensions
{
    public static Type[] GenericTypeArgumentsOrParameters(this Type type)
    {
        return type.IsGenericTypeDefinition ? type.GetTypeInfo().GenericTypeParameters : type.GenericTypeArguments;
    }

    public static Type GetGenericTypeDefinitionIfIsGenericType(this Type type)
    {
        return type.IsConstructedGenericType? type.GetGenericTypeDefinition() : type;
    }

    public static TMemberInfo GetMemberInfoInGenericDefType<TMemberInfo>(this Type type, TMemberInfo memberInfo) where TMemberInfo : MemberInfo
    {
        Type genericDefType = type.GetGenericTypeDefinitionIfIsGenericType();

        MemberInfo result = genericDefType.GetMemberWithSameMetadataDefinitionAs(memberInfo);
        return (TMemberInfo)result;
    }
}