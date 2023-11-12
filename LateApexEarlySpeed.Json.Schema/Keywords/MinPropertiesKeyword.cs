using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("minProperties")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinPropertiesKeyword>))]
internal class MinPropertiesKeyword : PropertiesSizeKeywordBase
{
    protected override bool IsSizeInRange(int instanceProperties)
    {
        return instanceProperties >= BenchmarkValue;
    }

    protected override string GetErrorMessage(int instanceProperties)
    {
        return $"Instance's property count is {instanceProperties} which is less than '{BenchmarkValue}'";
    }
}