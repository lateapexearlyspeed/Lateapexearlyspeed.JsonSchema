using System;
using System.Collections.Generic;
using System.Linq;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

public static class ValidationKeywordRegistry
{
    private static readonly Dictionary<string, Type> KeywordsDictionary;

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