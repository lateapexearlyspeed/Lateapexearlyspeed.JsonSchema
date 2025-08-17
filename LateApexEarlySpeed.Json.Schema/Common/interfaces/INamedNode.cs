using System.Diagnostics.CodeAnalysis;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

internal interface INamedNode
{
    [DisallowNull]
    string? Name { get; set; }
}