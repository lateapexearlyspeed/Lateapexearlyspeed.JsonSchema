using System.Reflection;
using LateApexEarlySpeed.Nullability.Generic;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class NullabilityFieldInfoWrapper : IFieldInfo
{
    private readonly NullabilityFieldInfo _field;

    public NullabilityFieldInfoWrapper(NullabilityFieldInfo field)
    {
        _field = field;
    }

    public MemberInfo MemberInfo => _field;
    public IType FieldType => new NullabilityTypeWrapper(_field.NullabilityFieldType);
}