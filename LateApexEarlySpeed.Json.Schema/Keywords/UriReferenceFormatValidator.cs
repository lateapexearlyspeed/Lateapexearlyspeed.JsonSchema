namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format(FormatName)]
internal class UriReferenceFormatValidator : FormatValidator
{
    public const string FormatName = "uri-reference";

    public override bool Validate(string content)
    {
        try
        {
            var _ = new Uri(content, UriKind.RelativeOrAbsolute);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}