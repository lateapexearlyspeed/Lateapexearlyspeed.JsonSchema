using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class MinObjectPropertiesAttribute : Attribute, IKeywordGenerator
{
    private readonly uint _minimumObjectProperties;

    public MinObjectPropertiesAttribute(uint minimumObjectProperties)
    {
        _minimumObjectProperties = minimumObjectProperties;
    }

    public KeywordBase CreateKeyword(Type type)
    {
        return new MinPropertiesKeyword { BenchmarkValue = _minimumObjectProperties };
    }
}