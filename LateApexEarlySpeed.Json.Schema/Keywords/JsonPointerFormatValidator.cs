using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("json-pointer")]
internal class JsonPointerFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        return LinkedListBasedImmutableJsonPointer.Create(content) is not null;
    }
}