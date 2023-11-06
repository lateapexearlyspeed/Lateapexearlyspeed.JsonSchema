namespace JsonSchemaConsoleApp.Keywords;

/// <summary>
/// Reference (usage) of keyword '$defs' are not specified whether it should be json pointer encoded or not in json schema doc,
/// so this library defines it should be json pointer encoded
/// </summary>
internal class DefsKeyword
{
    public const string Keyword = "$defs";

    private readonly Dictionary<string, JsonSchema> _definitions;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="definitions">Keys of it are short def name which is unescaped content</param>
    public DefsKeyword(Dictionary<string, JsonSchema> definitions)
    {
        _definitions = definitions.ToDictionary(kv => ConvertToDefNamePath(kv.Key), kv => kv.Value);
    }

    /// <param name="shortDefName">Unescaped content</param>
    /// <returns>Complete def path as escaped json pointer</returns>
    private static string ConvertToDefNamePath(string shortDefName)
    {
        return new JsonPointer(new[]{Keyword, shortDefName}).ToString();
    }

    /// <param name="defNameJsonPointerPath">Complete def path as escaped json pointer</param>
    /// <returns></returns>
    public JsonSchema? GetDefinition(string defNameJsonPointerPath)
    {
        return _definitions.GetValueOrDefault(defNameJsonPointerPath);
    }

    public Dictionary<string, JsonSchema> GetAllDefinitions()
    {
        return _definitions;
    }
}