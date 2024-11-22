using System.Reflection;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class NullabilityMethodInfoWrapper : IMethodInfo
{
    private readonly NullabilityMethodInfo _method;

    public NullabilityMethodInfoWrapper(NullabilityMethodInfo method)
    {
        _method = method;
    }

    public MemberInfo MemberInfo => _method;
    public MethodInfo MethodInfo => _method;
    public IParameterInfo ReturnParameter => new NullabilityParameterInfoWrapper(_method.NullabilityReturnParameter);
}