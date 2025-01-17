using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Xunit.Assertion.Json;

namespace JsonQuery.Net.UnitTests
{
    public class JsonFormatCompilerTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void CompileAndQuery(string category, string description, string input, string query, string expectedOutput)
        {
            IJsonQueryable jsonQueryable = new JsonFormatCompiler().Compile(query);

            JsonNode? actualJson = jsonQueryable.Query(JsonNode.Parse(input));
            JsonAssertion.Equivalent(expectedOutput, actualJson?.ToJsonString() ?? "null");
        }

        public static IEnumerable<object[]> TestCases
        {
            get
            {
                using (var fs = new FileStream(Path.Combine("test-suite", "compile.test.json"), FileMode.Open))
                {
                    using (JsonDocument jsonDoc = JsonDocument.Parse(fs))
                    {
                        JsonElement jsonElement = jsonDoc.RootElement.GetProperty("tests");

                        TestCase[] testCases = jsonElement.Deserialize<TestCase[]>()!;

                        return testCases.Select(testCase => new object[] { testCase.Category, testCase.Description, JsonSerializer.Serialize(testCase.Input), JsonSerializer.Serialize(testCase.Query), JsonSerializer.Serialize(testCase.ExpectedOutput) });
                    }
                }
                    
            }
        }

        public class TestCase
        {
            [JsonPropertyName("category")]
            public string Category { get; set; } = null!;

            [JsonPropertyName("description")]
            public string Description { get; set; } = null!;

            [JsonPropertyName("input")]
            public JsonNode? Input { get; set; }

            [JsonPropertyName("query")]
            public JsonNode? Query { get; set; }

            [JsonPropertyName("output")]
            public JsonNode? ExpectedOutput { get; set; }
        }
    }
}