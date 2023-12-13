using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class MaxObjectPropertiesAttribute : Attribute, IKeywordGenerator
{
    private readonly uint _maximumObjectProperties;

    public MaxObjectPropertiesAttribute(uint maximumObjectProperties)
    {
        _maximumObjectProperties = maximumObjectProperties;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new MaxPropertiesKeyword { BenchmarkValue = _maximumObjectProperties };
    }
}