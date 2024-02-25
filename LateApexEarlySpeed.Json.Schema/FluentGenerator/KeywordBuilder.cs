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

    internal virtual List<KeywordBase> Build()
    { 
        return Keywords.ToList();
    }
}