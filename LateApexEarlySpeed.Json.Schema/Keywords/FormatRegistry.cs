using System.Diagnostics;
using System.Reflection;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

public static class FormatRegistry
{
    private static readonly Dictionary<string, Type> FormatValidatorTypes;

    static FormatRegistry()
    {
        Type[] builtInFormatTypes = new[]
        {
            typeof(DateTimeFormatValidator),
            typeof(TimeFormatValidator),
            typeof(DateFormatValidator),
            typeof(EmailFormatValidator),
            typeof(HostNameFormatValidator),
            typeof(IPv4FormatValidator),
            typeof(IPv6FormatValidator),
            typeof(GuidFormatValidator),
            typeof(AbsoluteUriFormatValidator),
            typeof(UriReferenceFormatValidator),
            typeof(JsonPointerFormatValidator),
            typeof(RegexFormatValidator)
        };

        FormatValidatorTypes = builtInFormatTypes.ToDictionary(t =>
        {
            FormatAttribute? formatAttribute = t.GetCustomAttribute<FormatAttribute>();

            Debug.Assert(formatAttribute is not null);
            return formatAttribute.Name;
        });
    }

    public static void AddFormatType<TFormatValidator>() where TFormatValidator : FormatValidator
    {
        Type type = typeof(TFormatValidator);
        FormatAttribute? formatAttribute = type.GetCustomAttribute<FormatAttribute>();
        if (formatAttribute is null)
        {
            throw new ArgumentException($"Type argument: {type.FullName} should contain {nameof(FormatAttribute)}.", nameof(TFormatValidator));
        }

        FormatValidatorTypes.Add(formatAttribute.Name, type);
    }

    public static Type? GetFormatType(string format)
    {
        return FormatValidatorTypes.GetValueOrDefault(format);
    }
}