namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal interface IFieldInfo : IMemberInfo
{
    IType FieldType { get; }
}