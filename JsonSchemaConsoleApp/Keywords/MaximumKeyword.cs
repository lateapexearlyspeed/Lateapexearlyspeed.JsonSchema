using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("maximum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<MaximumKeyword>))]
internal class MaximumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue <= BenchmarkValue;
    }
}