using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("minimum")]
[JsonConverter(typeof(RangeKeywordBaseJsonConverter))]
internal class MinimumKeyword : RangeKeywordBase
{
    public MinimumKeyword(double minimum) : base(minimum)
    {
    }

    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue >= BenchmarkValue;
    }
}