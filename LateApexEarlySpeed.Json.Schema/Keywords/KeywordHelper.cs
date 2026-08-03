using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal static class KeywordHelper
{
    private static readonly ConcurrentDictionary<Type, string> NameForKeywordTypes = new();

    public static string GetKeywordName<TKeyword>() => GetKeywordName(typeof(TKeyword));

    public static string GetKeywordName(Type keywordType)
    {
        Debug.Assert(typeof(ValidationKeywordBase).IsAssignableFrom(keywordType) || typeof(AnnotationKeywordBase).IsAssignableFrom(keywordType));

        return NameForKeywordTypes.GetOrAdd(keywordType, type =>
        {
            Attribute? attr = type.GetCustomAttribute(typeof(KeywordAttribute));
            if (attr is null)
            {
                throw new BadKeywordException($"Type:{type.Name} should have {nameof(KeywordAttribute)}");
            }

            KeywordAttribute keywordAttr = (KeywordAttribute)attr;
            if (string.IsNullOrEmpty(keywordAttr.Name))
            {
                throw new BadKeywordException($"Type:{type.Name} should have {nameof(KeywordAttribute)} with non-empty {nameof(KeywordAttribute.Name)} property.");
            }

            return keywordAttr.Name;
        });
    }

    public static DialectKind[] GetKeywordDialects(Type keywordType)
    {
        Debug.Assert(typeof(ValidationKeywordBase).IsAssignableFrom(keywordType) || typeof(AnnotationKeywordBase).IsAssignableFrom(keywordType));

        DialectAttribute? dialectAttribute = keywordType.GetCustomAttribute<DialectAttribute>();

        if (dialectAttribute is null || dialectAttribute.Dialects.Length == 0)
        {
            Array sourceEnumValues = typeof(DialectKind).GetEnumValues();
            DialectKind[] allDialects = new DialectKind[sourceEnumValues.Length];
            Array.Copy(sourceEnumValues, allDialects, sourceEnumValues.Length);

            return allDialects;
        }

        DialectKind[] dialects = dialectAttribute.Dialects;

        for (int i = 0; i < dialects.Length - 1; i++)
        {
            DialectKind cur = dialects[i];

            for (int j = i + 1; j < dialects.Length; j++)
            {
                if (cur == dialects[j])
                {
                    throw new BadKeywordException($"Duplicated dialect '{cur}' found on type '{keywordType}'");
                }
            }
        }

        return dialects;
    }
}

public class BadKeywordException : Exception
{
    public BadKeywordException(string message) : base(message)
    {
    }
}