using System.Text.RegularExpressions;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("email")]
internal class EmailFormatValidator : FormatValidator
{
    private static readonly Regex EmailPattern = RegexFactory.Create("^[^@]+@([-w]+.)+[A-Za-z]{2,4}$", RegexOptions.Compiled);

    public override bool Validate(string content)
    {
        return EmailPattern.IsMatch(content);
    }
}