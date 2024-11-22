using System.Reflection;

namespace LateApexEarlySpeed.Nullability.Generic;

internal static class TypeExtensions
{
    public static Type[] GenericTypeArgumentsOrParameters(this Type type)
    {
        return type.GetGenericArguments();
    }

    public static Type GetGenericTypeDefinitionIfIsGenericType(this Type type)
    {
        return type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : type;
    }

    public static TMemberInfo GetMemberInfoInGenericDefType<TMemberInfo>(this Type type, TMemberInfo memberInfo) where TMemberInfo : MemberInfo
    {
        Type genericDefType = type.GetGenericTypeDefinitionIfIsGenericType();

        // In .net6, there is new sdk method: genericDefType.GetMemberWithSameMetadataDefinitionAs(memberInfo)
        MemberInfo result;
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Property:
                result = genericDefType.GetRuntimeProperties().First(prop => prop.HasSameMetadataDefinitionAs(memberInfo));
                break;
            case MemberTypes.Field:
                result = genericDefType.GetRuntimeFields().First(prop => prop.HasSameMetadataDefinitionAs(memberInfo));
                break;
            case MemberTypes.Method:
                result = genericDefType.GetRuntimeMethods().First(prop => prop.HasSameMetadataDefinitionAs(memberInfo));
                break;
            default:
                throw new NotSupportedException($"Method {nameof(GetMemberInfoInGenericDefType)} not support member type: {memberInfo.MemberType}");
        }

        return (TMemberInfo)result;
    }

    public static bool HasArrayElementType(this Type type) => type.IsArray && type.HasElementType;
}