namespace LateApexEarlySpeed.Json.Schema.Generator;

public class JsonSchemaNamingPolicy
{
    internal static JsonSchemaNamingPolicy SharedDefault { get; } = new();

    /// <summary>
    /// First word starts with a lower case character. Successive words start with an uppercase character.
    /// TempCelsius	=> tempCelsius
    /// </summary>
    public static JsonSchemaNamingPolicy CamelCase { get; } = new CamelCaseNamingPolicy();

    /// <summary>
    /// Words are separated by hyphens. All characters are lowercase.
    /// TempCelsius	-> temp-celsius
    /// </summary>
    public static JsonSchemaNamingPolicy KebabCaseLower { get; } = new KebabCaseLowerNamingPolicy();

    /// <summary>
    /// Words are separated by hyphens. All characters are uppercase.
    /// TempCelsius	=> TEMP-CELSIUS
    /// </summary>
    public static JsonSchemaNamingPolicy KebabCaseUpper { get; } = new KebabCaseUpperNamingPolicy();

    /// <summary>
    /// Words are separated by underscores. All characters are lowercase.
    /// TempCelsius	-> temp_celsius
    /// </summary>
    public static JsonSchemaNamingPolicy SnakeCaseLower { get; } = new SnakeCaseLowerNamingPolicy();

    /// <summary>
    /// Words are separated by underscores. All characters are uppercase.
    /// TempCelsius	-> TEMP_CELSIUS
    /// </summary>
    public static JsonSchemaNamingPolicy SnakeCaseUpper { get; } = new SnakeCaseUpperNamingPolicy();

    public virtual string ConvertName(string name)
    {
        return name;
    }
}