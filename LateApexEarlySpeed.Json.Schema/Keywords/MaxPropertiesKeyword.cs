using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("maxProperties")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MaxPropertiesKeyword>))]
internal class MaxPropertiesKeyword : PropertiesSizeKeywordBase
{
    protected override bool IsSizeInRange(int instanceProperties)
    {
        return instanceProperties <= BenchmarkValue;
    }
}