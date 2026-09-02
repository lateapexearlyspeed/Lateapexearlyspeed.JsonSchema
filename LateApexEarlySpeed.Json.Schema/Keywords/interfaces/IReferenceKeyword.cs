using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

public interface IReferenceKeyword : IValidationNode
{
    string Name { get; }
    Uri ParentResourceBaseUri { set; }
}