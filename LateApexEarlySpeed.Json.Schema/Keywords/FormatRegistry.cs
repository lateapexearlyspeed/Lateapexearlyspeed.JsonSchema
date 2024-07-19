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

    /// <summary>
    /// Add new format type <typeparamref name="TFormatValidator"/> to <see cref="FormatRegistry"/>
    /// </summary>
    /// <typeparam name="TFormatValidator">New format type to be added</typeparam>
    /// <exception cref="ArgumentException">An format type with the same name already exists in the <see cref="FormatRegistry"/></exception>
    public static void AddFormatType<TFormatValidator>() where TFormatValidator : FormatValidator
    {
        FormatValidatorTypes.Add(GetFormatName<TFormatValidator>(), typeof(TFormatValidator));
    }

    /// <summary>
    /// Set new format type <typeparamref name="TFormatValidator"/> to <see cref="FormatRegistry"/>.
    /// If specified format name does not exist, it is added; otherwise it is updated with new format type.
    /// </summary>
    /// <typeparam name="TFormatValidator">New format type to be set</typeparam>
    public static void SetFormatType<TFormatValidator>() where TFormatValidator : FormatValidator
    {
        FormatValidatorTypes[GetFormatName<TFormatValidator>()] = typeof(TFormatValidator);
    }

    private static string GetFormatName<TFormatValidator>() where TFormatValidator : FormatValidator
    {
        Type type = typeof(TFormatValidator);
        FormatAttribute? formatAttribute = type.GetCustomAttribute<FormatAttribute>();
        if (formatAttribute is null)
        {
            throw new ArgumentException($"Type argument: {type.FullName} should contain {nameof(FormatAttribute)}.", nameof(TFormatValidator));
        }

        return formatAttribute.Name;
    }

    /// <returns>Return <see cref="Type"/> for <paramref name="format"/> keyword if registered; otherwise return null</returns>
    public static Type? GetFormatType(string format)
    {
        return FormatValidatorTypes.GetValueOrDefault(format);
    }
}