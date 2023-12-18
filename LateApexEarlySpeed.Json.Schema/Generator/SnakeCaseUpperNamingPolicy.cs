namespace LateApexEarlySpeed.Json.Schema.Generator;

/// <summary>
/// Words are separated by underscores. All characters are uppercase.
/// TempCelsius	-> TEMP_CELSIUS
/// </summary>
internal class SnakeCaseUpperNamingPolicy : JsonSchemaNamingPolicy
{
    public override string ConvertName(string name)
    {
        return JsonSchemaNamingPolicyHelper.ConvertNameByTransformingCasingAndSeparator(name, false, '_');
    }
}