using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("maxItems")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MaxItemsKeyword>))]
internal class MaxItemsKeyword : ArrayLengthKeywordBase
{
    protected override bool IsSizeInRange(int instanceArrayLength)
    {
        return BenchmarkValue >= instanceArrayLength;
    }
}