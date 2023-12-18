namespace LateApexEarlySpeed.Json.Schema.Generator;

public class JsonSchemaNamingPolicy
{
    internal static JsonSchemaNamingPolicy SharedDefault { get; } = new();

    public static JsonSchemaNamingPolicy CamelCase { get; } = new CamelCaseNamingPolicy();
    
    public static JsonSchemaNamingPolicy KebabCaseLower { get; } = new KebabCaseLowerNamingPolicy();
    
    public static JsonSchemaNamingPolicy KebabCaseUpper { get; } = new KebabCaseUpperNamingPolicy();
    
    public static JsonSchemaNamingPolicy SnakeCaseLower { get; } = new SnakeCaseLowerNamingPolicy();
    
    public static JsonSchemaNamingPolicy SnakeCaseUpper { get; } = new SnakeCaseUpperNamingPolicy();

    public virtual string ConvertName(string name)
    {
        return name;
    }
}