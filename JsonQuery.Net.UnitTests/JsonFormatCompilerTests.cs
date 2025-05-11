using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JsonQuery.Net.Queryables;
using LateApexEarlySpeed.Xunit.Assertion.Json;

namespace JsonQuery.Net.UnitTests
{
    public class JsonFormatCompilerTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void CompileAndQuery(string category, string description, string input, string query, string expectedOutput)
        {
            IJsonQueryable jsonQueryable = JsonQueryable.Compile(query);

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
                        JsonElement jsonElement = jsonDoc.RootElement.GetProperty("groups");

                        Group[] groups = jsonElement.Deserialize<Group[]>()!;

                        return groups.SelectMany(group => group.Tests.Select(test => new object[] { group.Category, group.Description, JsonSerializer.Serialize(test.Input), JsonSerializer.Serialize(test.Query), JsonSerializer.Serialize(test.ExpectedOutput) }));
                    }
                }
            }
        }

        public class Group
        {
            [JsonPropertyName("category")]
            public string Category { get; set; } = null!;

            [JsonPropertyName("description")]
            public string Description { get; set; } = null!;

            [JsonPropertyName("tests")]
            public TestCase[] Tests { get; set; } = null!;
        }

        public class TestCase
        {
            [JsonPropertyName("input")]
            public JsonNode? Input { get; set; }

            [JsonPropertyName("query")]
            public JsonNode? Query { get; set; }

            [JsonPropertyName("output")]
            public JsonNode? ExpectedOutput { get; set; }
        }
    }
}