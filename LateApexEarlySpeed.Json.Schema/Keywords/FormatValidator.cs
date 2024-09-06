using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

public abstract class FormatValidator
{
    public static FormatValidator? Create(string format)
    {
        Type? formatType = FormatRegistry.GetFormatType(format);
        if (formatType is null)
        {
            return null;
        }

        object instance = Activator.CreateInstance(formatType)!;

        Debug.Assert(instance is FormatValidator);
        return instance as FormatValidator;
    }

    public abstract bool Validate(string content);
}