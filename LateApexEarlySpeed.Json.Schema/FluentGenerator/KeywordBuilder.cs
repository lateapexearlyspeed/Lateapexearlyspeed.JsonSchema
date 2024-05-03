using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class KeywordBuilder
{
    protected internal readonly List<KeywordBase> Keywords = new();

    public KeywordBuilder()
    {
    }

    protected KeywordBuilder(KeywordBase keyword)
    {
        Keywords.Add(keyword);
    }

    internal virtual KeywordCollection Build()
    { 
        return new KeywordCollection(Keywords.ToList());
    }
}

internal readonly struct KeywordCollection
{
    public KeywordCollection(List<KeywordBase> keywords, ArrayContainsValidator? arrayContainsValidator = null)
    {
        Keywords = keywords;
        ArrayContainsValidator = arrayContainsValidator;
    }

    public List<KeywordBase> Keywords { get; }
    public ArrayContainsValidator? ArrayContainsValidator { get; }
}