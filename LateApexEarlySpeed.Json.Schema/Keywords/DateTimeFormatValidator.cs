using System.Globalization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format(FormatName)]
internal class DateTimeFormatValidator : FormatValidator
{
    public const string FormatName = "date-time";

    private static readonly string[] Formats = new[]
    {
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ"
    };

    public override bool Validate(string content)
    {
        return DateTimeOffset.TryParseExact(content, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}