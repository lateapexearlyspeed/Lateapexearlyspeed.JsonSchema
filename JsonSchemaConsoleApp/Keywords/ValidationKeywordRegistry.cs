using System.Diagnostics;
using System.Reflection;

namespace JsonSchemaConsoleApp.Keywords;

static class ValidationKeywordRegistry
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
                typeof(NotKeyword)
            };
        KeywordsDictionary = builtInKeywordTypes.ToDictionary(GetKeywordName);
    }

    private static string GetKeywordName(Type keywordType)
    {
        KeywordAttribute? keywordAttr = keywordType.GetCustomAttribute(typeof(KeywordAttribute)) as KeywordAttribute;
        Debug.Assert(keywordAttr is not null);
        return keywordAttr.Name;
    }

    public static void AddKeyword(Type keywordType)
    {
        KeywordsDictionary.Add(GetKeywordName(keywordType), keywordType);
    }

    public static Type? GetKeyword(string keywordName)
    {
        return KeywordsDictionary.GetValueOrDefault(keywordName);
    }
}