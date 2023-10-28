using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("exclusiveMaximum")]
[JsonConverter(typeof(RangeKeywordBaseJsonConverter))]
internal class ExclusiveMaximumKeyword : RangeKeywordBase
{
    public ExclusiveMaximumKeyword(double exclusiveMaximum) : base(exclusiveMaximum)
    {
    }

    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue < BenchmarkValue;
    }
}