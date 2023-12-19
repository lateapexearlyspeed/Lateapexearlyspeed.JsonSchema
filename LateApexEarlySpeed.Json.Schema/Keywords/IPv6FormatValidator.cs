namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("ipv6")]
// ReSharper disable once InconsistentNaming
internal class IPv6FormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        return Uri.CheckHostName(content) == UriHostNameType.IPv6;
    }
}