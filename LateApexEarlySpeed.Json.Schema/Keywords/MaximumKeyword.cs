using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("maximum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<MaximumKeyword>))]
internal class MaximumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue) 
        => instanceValue <= BenchmarkValue;

    protected override string GetErrorMessage(double instanceValue)
        => $"Instance '{instanceValue}' is greater than '{BenchmarkValue}'";
}