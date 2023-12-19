using System.Globalization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("date-time")]
internal class DateTimeFormatValidator : FormatValidator
{
    private static readonly string[] Formats = new[]
    {
        "yyyy-MM-ddTHH:mm:ss.fzzz",
        "yyyy-MM-ddTHH:mm:ss.fZ",
        "yyyy-MM-ddTHH:mm:sszzz",
        "yyyy-MM-ddTHH:mm:ssZ"
    };

    public override bool Validate(string content)
    {
        return DateTimeOffset.TryParseExact(content, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}