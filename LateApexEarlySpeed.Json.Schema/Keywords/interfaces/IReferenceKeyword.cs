using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

public interface IReferenceKeyword : IValidationNode, IPropertyEvaluator, IItemEvaluator
{
    string Name { get; }
    Uri ParentResourceBaseUri { set; }
}