﻿using System.Reflection;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Obfuscation(ApplyToMembers = false)]
[Keyword("maxItems")]
[JsonConverter(typeof(BenchmarkValueKeywordJsonConverter<MaxItemsKeyword>))]
internal class MaxItemsKeyword : ArrayLengthKeywordBase
{
    protected override bool IsSizeInRange(int instanceArrayLength) 
        => BenchmarkValue >= instanceArrayLength;

    protected override string GetErrorMessage(int instanceArrayLength) 
        => ErrorMessage(instanceArrayLength, BenchmarkValue);

    [Obfuscation]
    public static string ErrorMessage(int instanceArrayLength, uint max)
    {
        return $"Array length: {instanceArrayLength} is greater than '{max}'";
    }
}