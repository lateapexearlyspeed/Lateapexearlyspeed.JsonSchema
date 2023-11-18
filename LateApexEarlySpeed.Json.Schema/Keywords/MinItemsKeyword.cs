using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("minItems")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinItemsKeyword>))]
internal class MinItemsKeyword : ArrayLengthKeywordBase
{
    protected override bool IsSizeInRange(int instanceArrayLength) 
        => BenchmarkValue <= instanceArrayLength;

    protected override string GetErrorMessage(int instanceArrayLength) 
        => $"Array length: {instanceArrayLength} is less than '{BenchmarkValue}'";
}