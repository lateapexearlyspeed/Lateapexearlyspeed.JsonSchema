namespace LateApexEarlySpeed.Json.Schema.Generator;

/// <summary>
/// Words are separated by hyphens. All characters are lowercase.
/// TempCelsius	-> temp-celsius
/// </summary>
internal class KebabCaseLowerNamingPolicy : JsonSchemaNamingPolicy
{
    public override string ConvertName(string name)
    {
        return JsonSchemaNamingPolicyHelper.ConvertNameByTransformingCasingAndSeparator(name, true, '-');
    }
}