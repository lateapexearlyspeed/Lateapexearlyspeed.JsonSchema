namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("uuid")]
internal class GuidFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        return Guid.TryParse(content, out _);
    }
}