using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("exclusiveMaximum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<ExclusiveMaximumKeyword>))]
internal class ExclusiveMaximumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue) 
        => instanceValue < BenchmarkValue;

    protected override string GetErrorMessage(double instanceValue)
        => ErrorMessage(instanceValue, BenchmarkValue);

    [Obfuscation]
    public static string ErrorMessage(double instanceValue, double maximum)
    {
        return $"Instance '{instanceValue}' is equal to or greater than '{maximum}'";
    }
}