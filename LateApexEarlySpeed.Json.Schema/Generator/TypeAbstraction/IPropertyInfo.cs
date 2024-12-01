namespace LateApexEarlySpeed.Json.Schema.Generator.TypeAbstraction;

internal interface IPropertyInfo : IMemberInfo
{
    IType PropertyType { get; }
}