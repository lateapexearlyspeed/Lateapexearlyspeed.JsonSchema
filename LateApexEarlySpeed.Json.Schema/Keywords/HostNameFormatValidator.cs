namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("hostname")]
internal class HostNameFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        return Uri.CheckHostName(content) == UriHostNameType.Dns;
    }
}