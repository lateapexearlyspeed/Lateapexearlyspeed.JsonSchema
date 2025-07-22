namespace LateApexEarlySpeed.Json.Schema.Keywords;

public static class ValidationKeywordRegistry
{
    private static readonly Dictionary<string, Type> KeywordsDictionary;

    private static readonly HashSet<string> IgnoredKeywordNames = new() { "$comment", "$schema", "$vocabulary", "contentEncoding", "contentMediaType", "contentSchema", "default", "deprecated", "description", "examples", "readOnly", "title", "writeOnly" };

    static ValidationKeywordRegistry()
    {
        var builtInKeywordTypes = new[] 
            { 
                typeof(AdditionalPropertiesKeyword),
                typeof(AllOfKeyword),
                typeof(AnyOfKeyword),
                typeof(ConstKeyword),
                typeof(DependentRequiredKeyword),
                typeof(DependentSchemasKeyword),
                typeof(EnumKeyword),
                typeof(ExclusiveMaximumKeyword),
                typeof(ExclusiveMinimumKeyword),
                typeof(FormatKeyword),
                typeof(ItemsKeyword),
                typeof(MaximumKeyword),
                typeof(MaxItemsKeyword),
                typeof(MaxLengthKeyword),
                typeof(MaxPropertiesKeyword),
                typeof(MinimumKeyword),
                typeof(MinItemsKeyword),
                typeof(MinLengthKeyword),
                typeof(MinPropertiesKeyword),
                typeof(MultipleOfKeyword),
                typeof(NotKeyword),
                typeof(OneOfKeyword),
                typeof(PatternKeyword),
                typeof(PatternPropertiesKeyword),
                typeof(PrefixItemsKeyword),
                typeof(PropertiesKeyword),
                typeof(PropertyNamesKeyword),
                typeof(RequiredKeyword),
                typeof(TypeKeyword),
                typeof(UniqueItemsKeyword)
            };
        KeywordsDictionary = builtInKeywordTypes.ToDictionary(KeywordBase.GetKeywordName);
    }

    /// <summary>
    /// Add new keyword type <typeparamref name="TKeyword"/> to <see cref="ValidationKeywordRegistry"/>
    /// </summary>
    /// <typeparam name="TKeyword">New keyword type to be added</typeparam>
    /// <exception cref="ArgumentException">An keyword type with the same keyword name already exists in the <see cref="ValidationKeywordRegistry"/></exception>
    public static void AddKeyword<TKeyword>() where TKeyword : KeywordBase
    {
        Type keywordType = typeof(TKeyword);
        KeywordsDictionary.Add(KeywordBase.GetKeywordName(keywordType), keywordType);
    }

    /// <summary>
    /// Set new keyword type <typeparamref name="TKeyword"/> to <see cref="ValidationKeywordRegistry"/>.
    /// If specified keyword name does not exist, it is added; otherwise it is updated with new keyword type.
    /// </summary>
    /// <typeparam name="TKeyword">New keyword type to be set</typeparam>
    public static void SetKeyword<TKeyword>() where TKeyword : KeywordBase
    {
        Type keywordType = typeof(TKeyword);
        KeywordsDictionary[KeywordBase.GetKeywordName(keywordType)] = keywordType;
    }

    public static Type? GetKeyword(string keywordName)
    {
        return KeywordsDictionary.GetValueOrDefault(keywordName);
    }

    public static bool ContainsIgnoredKeyword(string keywordName)
    {
        return IgnoredKeywordNames.Contains(keywordName);
    }
}