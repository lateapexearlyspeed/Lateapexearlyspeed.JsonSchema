using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class ParameterInfoWrapper : IParameterInfo
{
    private readonly ParameterInfo _parameterInfo;

    public ParameterInfoWrapper(ParameterInfo parameterInfo)
    {
        _parameterInfo = parameterInfo;
    }

    public IType ParameterType => new TypeWrapper(_parameterInfo.ParameterType);
}