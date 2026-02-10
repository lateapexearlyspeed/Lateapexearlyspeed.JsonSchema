using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

public abstract class KeywordBase : NamedValidationNode
// public abstract class KeywordBase<TKeyword> : NamedValidationNode where TKeyword : KeywordBase<TKeyword>
{
    // ReSharper disable once StaticMemberInGenericType
    // private static readonly string NameForKeywordType;
    private static readonly ConcurrentDictionary<Type, string> NameForKeywordTypes = new();

    /// <remarks><see cref="Name"/> here is always instantiated by constructor, so override it to make it 'non-nullable'</remarks>
    public sealed override string Name { get; set; }

    protected KeywordBase()
    {
        Type currentKeywordType = GetType();

        Name = GetKeywordName(currentKeywordType);
    }

    internal static string GetKeywordName(Type keywordType)
    {
        Debug.Assert(typeof(KeywordBase).IsAssignableFrom(keywordType));

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

    public static string GetKeywordName<TKeyword>() where TKeyword : KeywordBase
    {
        return GetKeywordName(typeof(TKeyword));
    }

    internal static DialectKind[] GetKeywordDialects(Type keywordType)
    {
        Debug.Assert(typeof(KeywordBase).IsAssignableFrom(keywordType));

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

    // static KeywordBase()
    // {
    //     Attribute? attr = typeof(TKeyword).GetCustomAttribute(typeof(KeywordAttribute));
    //     if (attr is null)
    //     {
    //         throw new BadKeywordException($"Type:{typeof(TKeyword).Name} should have {nameof(KeywordAttribute)}");
    //     }
    //
    //     KeywordAttribute keywordAttr = (KeywordAttribute)attr;
    //     if (string.IsNullOrEmpty(keywordAttr.Name))
    //     {
    //         throw new BadKeywordException($"Type:{typeof(TKeyword).Name} should have {nameof(KeywordAttribute)} with non-empty {nameof(KeywordAttribute.Name)} property.");
    //     }
    //
    //     NameForKeywordType = keywordAttr.Name;

    // }
}

public class BadKeywordException : Exception
{
    public BadKeywordException(string message) : base(message)
    {
    }
}