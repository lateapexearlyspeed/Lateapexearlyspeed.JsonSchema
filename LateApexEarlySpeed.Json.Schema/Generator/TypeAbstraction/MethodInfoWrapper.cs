using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class MethodInfoWrapper : IMethodInfo
{
    public MethodInfoWrapper(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
    }

    public MemberInfo MemberInfo => MethodInfo;
    public MethodInfo MethodInfo { get; }
    public IParameterInfo ReturnParameter => new ParameterInfoWrapper(MethodInfo.ReturnParameter!);
}