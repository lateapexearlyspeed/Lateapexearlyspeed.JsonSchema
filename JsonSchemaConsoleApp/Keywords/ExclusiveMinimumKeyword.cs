using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("exclusiveMinimum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<ExclusiveMinimumKeyword>))]
internal class ExclusiveMinimumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue > BenchmarkValue;
    }
}