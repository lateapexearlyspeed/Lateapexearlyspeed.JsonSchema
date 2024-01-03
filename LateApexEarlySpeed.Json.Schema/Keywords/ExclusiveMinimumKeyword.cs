using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("exclusiveMinimum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<ExclusiveMinimumKeyword>))]
internal class ExclusiveMinimumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue) 
        => instanceValue > BenchmarkValue;

    protected override string GetErrorMessage(double instanceValue)
        => ErrorMessage(instanceValue, BenchmarkValue);

    public static string ErrorMessage(double instanceValue, double minimum)
    {
        return $"Instance '{instanceValue}' is equal to or less than '{minimum}'";
    }
}