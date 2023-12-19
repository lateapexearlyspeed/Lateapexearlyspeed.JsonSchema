namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("uri-reference")]
internal class UriReferenceFormatValidator : FormatValidator
{
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