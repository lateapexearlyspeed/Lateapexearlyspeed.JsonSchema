using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("minItems")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinItemsKeyword>))]
internal class MinItemsKeyword : ArrayLengthKeywordBase
{
    protected override bool IsSizeInRange(int instanceArrayLength)
    {
        return BenchmarkValue <= instanceArrayLength;
    }
}