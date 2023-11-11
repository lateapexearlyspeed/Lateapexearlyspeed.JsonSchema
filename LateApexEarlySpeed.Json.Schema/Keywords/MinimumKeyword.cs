using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("minimum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<MinimumKeyword>))]
internal class MinimumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue >= BenchmarkValue;
    }
}