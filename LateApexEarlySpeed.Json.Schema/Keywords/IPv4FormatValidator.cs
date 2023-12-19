namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("ipv4")]
// ReSharper disable once InconsistentNaming
internal class IPv4FormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        return Uri.CheckHostName(content) == UriHostNameType.IPv4;
    }
}