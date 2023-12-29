namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format(FormatName)]
// ReSharper disable once InconsistentNaming
internal class IPv6FormatValidator : FormatValidator
{
    public const string FormatName = "ipv6";

    public override bool Validate(string content)
    {
        return Uri.CheckHostName(content) == UriHostNameType.IPv6;
    }
}