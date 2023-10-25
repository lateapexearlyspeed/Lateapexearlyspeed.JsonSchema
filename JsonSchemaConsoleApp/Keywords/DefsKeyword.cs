namespace JsonSchemaConsoleApp.Keywords;

public class DefsKeyword
{
    public const string Keyword = "$defs";

    private readonly Dictionary<string, JsonSchema> _definitions;

    public DefsKeyword(Dictionary<string, JsonSchema> definitions)
    {
        _definitions = definitions.ToDictionary(kv => ConvertToDefNamePath(kv.Key), kv => kv.Value);
    }

    private static string ConvertToDefNamePath(string shortDefName)
    {
        return new JsonPointer(new[]{Keyword, shortDefName}).ToString();
    }

    public JsonSchema? GetDefinition(string defNamePath)
    {
        return _definitions.GetValueOrDefault(defNamePath);
    }

    public Dictionary<string, JsonSchema> GetAllDefinitions()
    {
        return _definitions;
    }
}