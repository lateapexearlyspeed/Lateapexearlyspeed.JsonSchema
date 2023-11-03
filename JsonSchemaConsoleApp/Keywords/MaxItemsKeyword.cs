using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("maxItems")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MaxItemsKeyword>))]
internal class MaxItemsKeyword : ArrayLengthKeywordBase
{
    protected override bool IsSizeInRange(int instanceArrayLength)
    {
        return BenchmarkValue >= instanceArrayLength;
    }
}