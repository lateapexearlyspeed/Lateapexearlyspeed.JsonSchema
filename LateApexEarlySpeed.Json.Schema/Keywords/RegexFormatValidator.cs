using System.Text.RegularExpressions;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("regex")]
internal class RegexFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        try
        {
            var _ = RegexFactory.Create(content, RegexOptions.None);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}