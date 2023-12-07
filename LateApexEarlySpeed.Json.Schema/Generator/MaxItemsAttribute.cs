using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
public class MaxItemsAttribute : Attribute, IKeywordGenerator
{
    public uint MaxItems { get; }

    public MaxItemsAttribute(uint maxItems)
    {
        MaxItems = maxItems;
    }

    public KeywordBase CreateKeyword()
    {
        return new MaxItemsKeyword { BenchmarkValue = MaxItems };
    }
}

public interface IKeywordGenerator
{
    public KeywordBase CreateKeyword();
}