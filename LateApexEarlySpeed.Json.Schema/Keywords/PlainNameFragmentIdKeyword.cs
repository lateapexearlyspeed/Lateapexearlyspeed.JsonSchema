using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

internal class PlainNameFragmentIdKeyword : IPlainNameIdentifierKeyword
{
    private readonly string _escapedPlainNameWithoutNumberSign;

    public PlainNameFragmentIdKeyword(string escapedPlainNameWithoutNumberSign)
    {
        _escapedPlainNameWithoutNumberSign = escapedPlainNameWithoutNumberSign;
    }

    public string KeywordName => IdKeyword.Keyword;
    public string Identifier => Uri.UnescapeDataString(_escapedPlainNameWithoutNumberSign);
    public string SerializedValue => "#" + _escapedPlainNameWithoutNumberSign;
}