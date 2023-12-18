namespace LateApexEarlySpeed.Json.Schema.Generator;

/// <summary>
/// First word starts with a lower case character. Successive words start with an uppercase character.
/// TempCelsius	=> tempCelsius
/// </summary>
internal class CamelCaseNamingPolicy : JsonSchemaNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (name.Length == 0 || !char.IsUpper(name, 0))
        {
            return name;
        }

        char[] buffer = name.ToCharArray();
        buffer[0] = char.ToLowerInvariant(buffer[0]);

        return new string(buffer);
    }
}