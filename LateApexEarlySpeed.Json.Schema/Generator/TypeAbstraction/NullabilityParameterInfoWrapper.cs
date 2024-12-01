using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class NullabilityParameterInfoWrapper : IParameterInfo
{
    private readonly NullabilityParameterInfo _parameter;

    public NullabilityParameterInfoWrapper(NullabilityParameterInfo parameter)
    {
        _parameter = parameter;
    }

    public IType ParameterType => new NullabilityTypeWrapper(_parameter.NullabilityParameterType);
}