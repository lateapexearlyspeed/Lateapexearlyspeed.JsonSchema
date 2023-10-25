namespace JsonSchemaConsoleApp;

abstract class StringFormatParser
{
    public abstract bool IsInstanceValid(string instance);

    public static StringFormatParser Create(string format)
    {
        if (format == "date-time")
        {
            return new DateTimeFormatParser();
        }

        throw new NotSupportedException($"format:{format} is not supported currently.");
    }
}