namespace LateApexEarlySpeed.Json.Schema.Keywords;

public static class ValidationKeywordRegistry
{
    private static readonly Dictionary<string, Type> KeywordsDictionary;

    static ValidationKeywordRegistry()
    {
        var builtInKeywordTypes = new[] 
            { 
                typeof(TypeKeyword), 
                typeof(MultipleOfKeyword), 
                typeof(PropertiesKeyword), 
                typeof(AllOfKeyword), 
                typeof(AnyOfKeyword), 
                typeof(OneOfKeyword), 
                typeof(NotKeyword),
                typeof(MaximumKeyword)
            };
        KeywordsDictionary = builtInKeywordTypes.ToDictionary(KeywordBase.GetKeywordName);
    }

    public static void AddKeyword<TKeyword>() where TKeyword : KeywordBase
    {
        Type keywordType = typeof(TKeyword);
        KeywordsDictionary.Add(KeywordBase.GetKeywordName(keywordType), keywordType);
    }

    public static Type? GetKeyword(string keywordName)
    {
        return KeywordsDictionary.GetValueOrDefault(keywordName);
    }
}