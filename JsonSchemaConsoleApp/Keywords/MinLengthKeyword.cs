using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("minLength")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinLengthKeyword>))]
internal class MinLengthKeyword : StringLengthKeywordBase
{
    protected override bool IsStringLengthInRange(int instanceStringLength)
    {
        return instanceStringLength >= BenchmarkValue;
    }
}