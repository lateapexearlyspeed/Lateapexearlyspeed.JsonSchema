using System.Globalization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("time")]
internal class TimeFormatValidator : FormatValidator
{
    private static readonly string[] Formats = new[]
    {
        "HH:mm:ss.FFFFFFFzzz",
        "HH:mm:ss.FFFFFFFZ",

        "HH:mm:ss.FFFFFFF"
    };

    public override bool Validate(string content)
    {
        return DateTimeOffset.TryParseExact(content, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}