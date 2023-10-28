using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("exclusiveMinimum")]
[JsonConverter(typeof(RangeKeywordBaseJsonConverter))]
internal class ExclusiveMinimumKeyword : RangeKeywordBase
{
    public ExclusiveMinimumKeyword(double exclusiveMinimum) : base(exclusiveMinimum)
    {
    }

    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue > BenchmarkValue;
    }
}