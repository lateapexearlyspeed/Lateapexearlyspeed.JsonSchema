using System.Diagnostics;
using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal static class MemberInfoExtensions
{
    public static IType GetMemberType(this IMemberInfo memberInfo)
    {
        MemberTypes memberType = memberInfo.MemberInfo.MemberType;
        Debug.Assert(memberType == MemberTypes.Property || memberType == MemberTypes.Field);

        return memberType == MemberTypes.Property
            ? ((IPropertyInfo)memberInfo).PropertyType
            : ((IFieldInfo)memberInfo).FieldType;
    }
}