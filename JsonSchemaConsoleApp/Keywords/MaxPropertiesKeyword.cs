using JsonSchemaConsoleApp.JsonConverters;
using System.Text.Json.Serialization;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("maxProperties")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MaxPropertiesKeyword>))]
internal class MaxPropertiesKeyword : PropertiesSizeKeywordBase
{
    protected override bool IsSizeInRange(int instanceProperties)
    {
        return instanceProperties <= BenchmarkValue;
    }
}