using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("maximum")]
[JsonConverter(typeof(RangeKeywordBaseJsonConverter))]
public class MaximumKeyword : RangeKeywordBase
{
    public MaximumKeyword(double maximum) : base(maximum)
    {
    }

    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue <= BenchmarkValue;
    }
}