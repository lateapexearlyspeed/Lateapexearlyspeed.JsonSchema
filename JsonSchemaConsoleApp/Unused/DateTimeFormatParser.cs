namespace JsonSchemaConsoleApp;

internal class DateTimeFormatParser : StringFormatParser
{
    public override bool IsInstanceValid(string instance)
    {
        return DateTimeOffset.TryParse(instance, out _);
    }
}