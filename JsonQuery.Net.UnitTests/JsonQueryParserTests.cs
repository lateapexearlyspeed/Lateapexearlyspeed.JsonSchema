using System.Text.Json;
using LateApexEarlySpeed.Xunit.Assertion.Json;

namespace JsonQuery.Net.UnitTests;

public class JsonQueryParserTests
{
    [Theory]
    [MemberData(nameof(ValidJsonQueryData))]
    public void Parse_ValidQuery_ShouldReturn(string jsonQuery, string expectedJsonFormat)
    {
        IJsonQueryable jsonQueryable = JsonQueryParser.Parse(jsonQuery);
        string actualJsonFormat = jsonQueryable.SerializeToJsonFormat();

        JsonAssertion.Equivalent(expectedJsonFormat, actualJsonFormat);
    }

    [Theory]
    [MemberData(nameof(InvalidJsonQueryData))]
    public void Parse_InvalidQuery_ShouldThrow(string jsonQuery)
    {
        Assert.Throws<JsonQueryParseException>(() => JsonQueryParser.Parse(jsonQuery));
    }

    public static IEnumerable<object[]> ValidJsonQueryData => GetParseTestData(true);
    public static IEnumerable<object[]> InvalidJsonQueryData => GetParseTestData(false);

    private static IEnumerable<object[]> GetParseTestData(bool validCase)
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