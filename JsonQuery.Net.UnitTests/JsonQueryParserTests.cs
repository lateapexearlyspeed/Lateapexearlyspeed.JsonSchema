﻿using System.Text.Json;
using JsonQuery.Net.Queryables;
using LateApexEarlySpeed.Xunit.Assertion.Json;

namespace JsonQuery.Net.UnitTests;

public class JsonQueryParserTests
{
    [Theory]
    [MemberData(nameof(ValidJsonQueryData))]
    public void Parse_ValidQuery_ShouldReturn(string jsonQuery, string expectedJsonFormat)
    {
        IJsonQueryable jsonQueryable = JsonQueryable.Parse(jsonQuery);
        string actualJsonFormat = jsonQueryable.SerializeToJsonFormat();

        JsonAssertion.Equivalent(expectedJsonFormat, actualJsonFormat);
    }

    [Theory]
    [MemberData(nameof(InvalidJsonQueryData))]
    public void Parse_InvalidQuery_ShouldThrow(string jsonQuery)
    {
        Assert.Throws<JsonQueryParseException>(() => JsonQueryable.Parse(jsonQuery));
    }

    public static IEnumerable<object[]> ValidJsonQueryData => GetParseTestDataFromTestSuite(true).Concat(GetAllFunctionsParseTestData());

    private static IEnumerable<object[]> GetAllFunctionsParseTestData()
    {
        using (var fs = new FileStream("all_functions.parse.test.json", FileMode.Open))
        {
            using (JsonDocument jsonDoc = JsonDocument.Parse(fs))
            {
                foreach (JsonElement testCase in jsonDoc.RootElement.EnumerateArray())
                {
                    yield return new object[] { testCase.GetProperty("query").GetString()!, JsonSerializer.Serialize(testCase.GetProperty("json")) };
                }
            }
        }
    }

    public static IEnumerable<object[]> InvalidJsonQueryData => GetParseTestDataFromTestSuite(false);

    private static IEnumerable<object[]> GetParseTestDataFromTestSuite(bool validCase)
    {
        using (var fs = new FileStream(Path.Combine("test-suite", "parse.test.json"), FileMode.Open))
        {
            using (JsonDocument jsonDoc = JsonDocument.Parse(fs))
            {
                foreach (JsonElement group in jsonDoc.RootElement.GetProperty("groups").EnumerateArray())
                {
                    foreach (JsonElement testCase in group.GetProperty("tests").EnumerateArray())
                    {
                        if (IsInBlacklist(testCase.GetProperty("input").GetString()!))
                        {
                            continue;
                        }

                        if (validCase)
                        {
                            if (!testCase.TryGetProperty("throws", out _))
                            {
                                yield return new object[] { testCase.GetProperty("input").GetString()!, JsonSerializer.Serialize(testCase.GetProperty("output")) };
                            }
                        }
                        else
                        {
                            if (testCase.TryGetProperty("throws", out _))
                            {
                                yield return new object[] { testCase.GetProperty("input").GetString()! };
                            }
                        }
                    }
                }
            }
        }
    }

    private static bool IsInBlacklist(string jsonQuery)
    {
        return jsonQuery == "sort(get())" || jsonQuery == "2." || jsonQuery == ".01" || jsonQuery == "{a:2,}";
    }
}