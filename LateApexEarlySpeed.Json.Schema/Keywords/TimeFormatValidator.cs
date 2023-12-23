using System.Globalization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("time")]
internal class TimeFormatValidator : FormatValidator
{
    private static readonly string[] Formats = new[]
    {
        "HH:mm:ss.fzzz",
        "HH:mm:ss.fZ",
        "HH:mm:sszzz",
        "HH:mm:ssZ",

        "HH:mm:ss.f",
        "HH:mm:ss",
    };

    public override bool Validate(string content)
    {
        return DateTimeOffset.TryParseExact(content, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}