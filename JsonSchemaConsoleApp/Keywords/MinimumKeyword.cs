using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("minimum")]
[JsonConverter(typeof(NumberRangeKeywordJsonConverter<MinimumKeyword>))]
internal class MinimumKeyword : NumberRangeKeywordBase
{
    protected override bool IsInRange(double instanceValue)
    {
        return instanceValue >= BenchmarkValue;
    }
}