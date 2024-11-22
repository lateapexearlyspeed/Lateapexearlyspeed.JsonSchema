using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal class FieldInfoWrapper : IFieldInfo
{
    private readonly FieldInfo _fieldInfo;

    public FieldInfoWrapper(FieldInfo fieldInfo)
    {
        _fieldInfo = fieldInfo;
    }

    public MemberInfo MemberInfo => _fieldInfo;
    public IType FieldType => new TypeWrapper(_fieldInfo.FieldType);
}