using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

public interface IMemberInfo
{
    MemberInfo MemberInfo { get; }
}