using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("minLength")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MinLengthKeyword>))]
internal class MinLengthKeyword : StringLengthKeywordBase
{
    protected override bool IsStringLengthInRange(int instanceStringLength)
    {
        return instanceStringLength >= BenchmarkValue;
    }

    protected override string GetErrorMessage(int instanceStringLength)
    {
        return ErrorMessage(instanceStringLength, BenchmarkValue);
    }

    [Obfuscation]
    public static string ErrorMessage(int instanceLength, uint minLength)
    {
        return $"String instance's length is {instanceLength} which is less than '{minLength}'";
    }
}