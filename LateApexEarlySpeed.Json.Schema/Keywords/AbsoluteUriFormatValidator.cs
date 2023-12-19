namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("uri")]
internal class AbsoluteUriFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        try
        {
            var _ = new Uri(content, UriKind.Absolute);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}