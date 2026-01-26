using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

public interface IReferenceKeyword : INamedNode, IValidationNode
{
    Uri ParentResourceBaseUri { set; }
}