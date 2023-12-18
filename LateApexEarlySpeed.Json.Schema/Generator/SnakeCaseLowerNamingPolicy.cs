namespace LateApexEarlySpeed.Json.Schema.Generator;

/// <summary>
/// Words are separated by underscores. All characters are lowercase.
/// TempCelsius	-> temp_celsius
/// </summary>
internal class SnakeCaseLowerNamingPolicy : JsonSchemaNamingPolicy
{
    public override string ConvertName(string name)
    {
        return JsonSchemaNamingPolicyHelper.ConvertNameByTransformingCasingAndSeparator(name, true, '_');
    }
}