using System.Globalization;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Format("date")]
internal class DateFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        return DateTimeOffset.TryParseExact(content, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}