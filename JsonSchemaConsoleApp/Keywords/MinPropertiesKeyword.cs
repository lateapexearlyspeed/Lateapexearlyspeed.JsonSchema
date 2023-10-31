using JsonSchemaConsoleApp.JsonConverters;
using System.Text.Json.Serialization;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("minProperties")]
[JsonConverter(typeof(PropertiesSizeKeywordJsonConverter<MinPropertiesKeyword>))]
internal class MinPropertiesKeyword : PropertiesSizeKeywordBase
{
    protected override bool IsSizeInRange(int instanceProperties)
    {
        return instanceProperties >= PropertiesBenchmark;
    }
}