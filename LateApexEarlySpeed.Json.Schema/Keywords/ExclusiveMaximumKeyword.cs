using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("exclusiveMaximum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<ExclusiveMaximumKeyword>))]
internal class ExclusiveMaximumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue) 
        => instanceValue < BenchmarkValue;

    protected override string GetErrorMessage(double instanceValue)
        => $"Instance '{instanceValue}' is equal to or greater than '{BenchmarkValue}'";
}