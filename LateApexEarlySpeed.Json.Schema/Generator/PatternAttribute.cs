using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PatternAttribute : Attribute, IKeywordGenerator
{
    private readonly string _pattern;

    public PatternAttribute(string pattern)
    {
        _pattern = pattern;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new PatternKeyword(_pattern);
    }
}