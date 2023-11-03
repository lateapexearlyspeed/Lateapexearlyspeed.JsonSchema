using JsonSchemaConsoleApp.JsonConverters;
using System.Text.Json.Serialization;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("minProperties")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinPropertiesKeyword>))]
internal class MinPropertiesKeyword : PropertiesSizeKeywordBase
{
    protected override bool IsSizeInRange(int instanceProperties)
    {
        return instanceProperties >= BenchmarkValue;
    }
}