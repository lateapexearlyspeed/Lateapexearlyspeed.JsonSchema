using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("maxLength")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MaxLengthKeyword>))]
internal class MaxLengthKeyword : StringLengthKeywordBase
{
    protected override bool IsStringLengthInRange(int instanceStringLength)
    {
        return instanceStringLength <= BenchmarkValue;
    }
}