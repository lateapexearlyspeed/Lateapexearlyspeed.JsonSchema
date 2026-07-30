using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

public abstract class KeywordBase : NamedValidationNode
// public abstract class KeywordBase<TKeyword> : NamedValidationNode where TKeyword : KeywordBase<TKeyword>
{
    // ReSharper disable once StaticMemberInGenericType
    // private static readonly string NameForKeywordType;

    /// <remarks><see cref="Name"/> here is always instantiated by constructor, so override it to make it 'non-nullable'</remarks>
    public sealed override string Name { get; set; }

    protected KeywordBase()
    {
        Type currentKeywordType = GetType();

        Name = KeywordHelper.GetKeywordName(currentKeywordType);
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
