using System.Text.Json.Nodes;
using JsonQuery.Net.Queryables;
using LateApexEarlySpeed.Xunit.Assertion.Json;

namespace JsonQuery.Net.UnitTests;

public class DemoTests
{
    [Theory]
    [MemberData(nameof(DemoData))]
    public void TestDemoData(string input, string jsonQuery, string expectedJsonFormatQuery, string expectedOutput)
    {
        // Use jsonQuery to query
        IJsonQueryable queryable = JsonQueryable.Parse(jsonQuery);

        string actualOutput = queryable.Query(JsonNode.Parse(input))!.ToJsonString();

        JsonAssertion.Equivalent(expectedOutput, actualOutput);

        // Transfer from jsonQuery to jsonFormat
        string actualJsonFormatQuery = queryable.SerializeToJsonFormat();

        JsonAssertion.Equivalent(expectedJsonFormatQuery, actualJsonFormatQuery);

        // Use jsonFormatQuery to query
        queryable = JsonQueryable.Compile(actualJsonFormatQuery);

        actualOutput = queryable.Query(JsonNode.Parse(input))!.ToJsonString();

        JsonAssertion.Equivalent(expectedOutput, actualOutput);
    }

    public static IEnumerable<object[]> DemoData
    {
        get
        {
            yield return new object[]
            {
                """
                {
                  "friends": [
                    {"name": "Chris", "age": 23, "city": "New York"},
                    {"name": "Emily", "age": 19, "city": "Atlanta"},
                    {"name": "Joe", "age": 32, "city": "New York"},
                    {"name": "Kevin", "age": 19, "city": "Atlanta"},
                    {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                    {"name": "Robert", "age": 45, "city": "Manhattan"},
                    {"name": "Sarah", "age": 31, "city": "New York"}
                  ]
                }
                """,
                """
                .friends
                | filter(.city == "New York")
                | sort(.age)
                | pick(.name, .age)
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["filter", ["eq", ["get", "city"], "New York"]],
                  ["sort", ["get", "age"]],
                  ["pick", ["get", "name"], ["get", "age"]]
                ]
                """,
                """
                [
                  {"name": "Chris", "age": 23},
                  {"name": "Sarah", "age": 31},
                  {"name": "Joe"  , "age": 32}
                ]
                """
            };

            yield return new object[]
            {
                """
                [
                  {"name": "Chris", "age": 23, "city": "New York"},
                  {"name": "Emily", "age": 19, "city": "Atlanta"},
                  {"name": "Joe", "age": 32, "city": "New York"},
                  {"name": "Kevin", "age": 19, "city": "Atlanta"},
                  {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                  {"name": "Robert", "age": 45, "city": "Manhattan"},
                  {"name": "Sarah", "age": 31, "city": "New York"}
                ]
                """,
                """
                filter((.city == "New York") and (.age > 30))
                """,
                """
                [
                  "filter",
                  [
                    "and",
                    ["eq", ["get", "city"], "New York"],
                    ["gt", ["get", "age"], 30]
                  ]
                ]
                """,
                """
                [
                  {"name": "Joe"  , "age": 32, "city": "New York"},
                  {"name": "Sarah", "age": 31, "city": "New York"}
                ]
                """
            };

            yield return new object[]
            {
                """
                [
                  {"name": "Chris", "age": 23, "city": "New York"},
                  {"name": "Emily", "age": 19, "city": "Atlanta"},
                  {"name": "Joe", "age": 32, "city": "New York"},
                  {"name": "Kevin", "age": 19, "city": "Atlanta"},
                  {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                  {"name": "Robert", "age": 45, "city": "Manhattan"},
                  {"name": "Sarah", "age": 31, "city": "New York"}
                ]
                """,
                """
                map({
                  firstName: .name,
                  details: {
                    city: .city,
                    age: .age
                  }
                })
                """,
                """
                [
                  "map",
                  [
                    "object",
                    {
                      "firstName": ["get", "name"],
                      "details": [
                        "object",
                        {
                          "city": ["get", "city"],
                          "age" : ["get", "age" ]
                        }
                      ]
                    }
                  ]
                ]
                """,
                """
                [
                  {
                    "firstName": "Chris",
                    "details": {"city": "New York", "age": 23}
                  },
                  {
                    "firstName": "Emily",
                    "details": {"city": "Atlanta", "age": 19}
                  },
                  {
                    "firstName": "Joe",
                    "details": {"city": "New York", "age": 32}
                  },
                  {
                    "firstName": "Kevin",
                    "details": {"city": "Atlanta", "age": 19}
                  },
                  {
                    "firstName": "Michelle",
                    "details": {"city": "Los Angeles", "age": 27}
                  },
                  {
                    "firstName": "Robert",
                    "details": {"city": "Manhattan", "age": 45}
                  },
                  {
                    "firstName": "Sarah",
                    "details": {"city": "New York", "age": 31}
                  }
                ]
                """
            };

            yield return new object[]
            {
                """
                [
                  {"name": "Chris", "age": 23, "city": "New York"},
                  {"name": "Emily", "age": 19, "city": "Atlanta"},
                  {"name": "Joe", "age": 32, "city": "New York"},
                  {"name": "Kevin", "age": 19, "city": "Atlanta"},
                  {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                  {"name": "Robert", "age": 45, "city": "Manhattan"},
                  {"name": "Sarah", "age": 31, "city": "New York"}
                ]
                """,
                """
                {
                  names: map(.name),
                  count: size(),
                  averageAge: map(.age) | average()
                }
                """,
                """
                [
                  "object",
                  {
                    "names": ["map", ["get", "name"]],
                    "count": ["size"],
                    "averageAge": [
                      "pipe",
                      ["map", ["get", "age"]],
                      ["average"]
                    ]
                  }
                ]
                """,
                """
                {
                  "names": [
                    "Chris"   , "Emily"   , "Joe"     , "Kevin"   ,
                    "Michelle", "Robert"  , "Sarah"
                  ],
                  "count": 7,
                  "averageAge": 28
                }
                """
            };

            yield return new object[]
            {
                """
                [
                  { "name": "bread", "price": 2.5, "quantity": 2 },
                  { "name": "milk", "price": 1.2, "quantity": 3 }
                ]
                """,
                """
                map(.price * .quantity) | sum()
                """,
                """
                [
                  "pipe",
                  [
                    "map",
                    ["multiply", ["get", "price"], ["get", "quantity"]]
                  ],
                  ["sum"]
                ]
                """,
                "8.6"
            };
        }
    }
}