using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator;

public class KeywordBuilder
{
    protected internal readonly List<ValidationKeywordBase> Keywords = new();

    public KeywordBuilder()
    {
    }

    protected KeywordBuilder(ValidationKeywordBase keyword)
    {
        Keywords.Add(keyword);
    }

    internal virtual KeywordCollection Build()
    { 
        return new KeywordCollection(Keywords);
    }
}

internal readonly struct KeywordCollection
{
    public KeywordCollection(List<ValidationKeywordBase> keywords, ArrayContainsValidator? arrayContainsValidator = null)
    {
        Keywords = keywords;
        ArrayContainsValidator = arrayContainsValidator;
    }

    public List<ValidationKeywordBase> Keywords { get; }
    public ArrayContainsValidator? ArrayContainsValidator { get; }
}