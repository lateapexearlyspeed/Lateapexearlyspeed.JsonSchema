using System.Globalization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format(FormatName)]
internal class DotNetDateTimeFormatValidator : FormatValidator
{
    public const string FormatName = "dotnet-date-time";

    private static readonly string[] Formats = new[]
    {
        "yyyy-MM-ddTHH:mm:ss.f",
        "yyyy-MM-ddTHH:mm:ss"
    };

    public override bool Validate(string content)
    {
        return DateTime.TryParseExact(content, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}