namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format(FormatName)]
// ReSharper disable once InconsistentNaming
internal class IPv4FormatValidator : FormatValidator
{
    public const string FormatName = "ipv4";

    public override bool Validate(string content)
    {
        return Uri.CheckHostName(content) == UriHostNameType.IPv4;
    }
}