namespace LateApexEarlySpeed.Json.Schema.Generator;

/// <summary>
/// Words are separated by hyphens. All characters are uppercase.
/// TempCelsius	=> TEMP-CELSIUS
/// </summary>
internal class KebabCaseUpperNamingPolicy : JsonSchemaNamingPolicy
{
    public override string ConvertName(string name)
    {
        return JsonSchemaNamingPolicyHelper.ConvertNameByTransformingCasingAndSeparator(name, false, '-');
    }
}