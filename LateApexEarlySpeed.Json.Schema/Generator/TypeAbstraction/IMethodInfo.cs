using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal interface IMethodInfo : IMemberInfo
{
    MethodInfo MethodInfo { get; }
    IParameterInfo ReturnParameter { get; }
}