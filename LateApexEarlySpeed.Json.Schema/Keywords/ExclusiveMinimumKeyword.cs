using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("exclusiveMinimum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<ExclusiveMinimumKeyword>))]
internal class ExclusiveMinimumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue > BenchmarkValue;
    }
}