using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class AnchorKeyword : IPlainNameIdentifierKeyword
{
    public const string Keyword = "$anchor";

    public string Identifier { get; }

    public AnchorKeyword(string identifier)
    {
        Identifier = identifier;
    }

    public string KeywordName => Keyword;
    public string SerializedValue => Identifier;
}