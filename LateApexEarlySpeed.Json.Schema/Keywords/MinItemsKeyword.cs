using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("minItems")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinItemsKeyword>))]
internal class MinItemsKeyword : ArrayLengthKeywordBase
{
    protected override bool IsSizeInRange(int instanceArrayLength) 
        => BenchmarkValue <= instanceArrayLength;

    protected override string GetErrorMessage(int instanceArrayLength) 
        => ErrorMessage(instanceArrayLength, BenchmarkValue);

    [Obfuscation]
    public static string ErrorMessage(int instanceArrayLength, uint min)
    {
        return $"Array length: {instanceArrayLength} is less than '{min}'";
    }
}