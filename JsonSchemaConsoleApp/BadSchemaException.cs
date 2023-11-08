namespace JsonSchemaConsoleApp;

public class BadSchemaException : Exception
{
    public BadSchemaException(string message) : base(message)
    {
    }
}