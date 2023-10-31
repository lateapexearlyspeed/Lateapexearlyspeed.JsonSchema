using JsonSchemaConsoleApp.JsonConverters;
using System.Text.Json.Serialization;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("exclusiveMaximum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<ExclusiveMaximumKeyword>))]
internal class ExclusiveMaximumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue < BenchmarkValue;
    }
}