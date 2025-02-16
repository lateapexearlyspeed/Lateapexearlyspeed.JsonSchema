using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JsonQuery.Net.Queryables;
using LateApexEarlySpeed.Xunit.Assertion.Json;

namespace JsonQuery.Net.UnitTests;

public class CompilerAndParserAndQueryTests : IClassFixture<CustomFunctionTestFixture>
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void TestCompilerAndParserAndQuery(string input, string jsonQuery, string expectedJsonFormatQuery, string expectedOutput)
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

    public static IEnumerable<object[]> TestData => DemoData.Concat(CustomFunctionData);

    private static IEnumerable<object[]> CustomFunctionData
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
                    {"name": "Kevin", "age": 19, "city": "Atlanta"},
                    {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                    {"name": "Robert", "age": 45, "city": "Manhattan"}
                  ]
                }
                """,
                """
                .friends 
                | sort(.age)
                | any(.city == "New York")
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["any", ["eq", ["get", "city"], "New York"]]
                ]
                """,
                """
                true
                """
            };

            yield return new object[]
            {
                """
                {
                  "friends": [
                    {"name": "Chris", "age": 23, "city": "Beijing"},
                    {"name": "Emily", "age": 19, "city": "Atlanta"},
                    {"name": "Kevin", "age": 19, "city": "Atlanta"},
                    {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                    {"name": "Robert", "age": 45, "city": "Manhattan"}
                  ]
                }
                """,
                """
                .friends 
                | sort(.age)
                | any(.city == "New York")
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["any", ["eq", ["get", "city"], "New York"]]
                ]
                """,
                """
                false
                """
            };

            yield return new object[]
            {
                """
                {
                  "friends": [
                    {"name": "Chris", "age": 23, "city": "New York"},
                    {"name": "Emily", "age": 19, "city": "Atlanta"},
                    {"name": "Kevin", "age": 19, "city": "Atlanta"},
                    {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                    {"name": "Robert", "age": 45, "city": "Manhattan"}
                  ]
                }
                """,
                """
                .friends 
                | sort(.age)
                | all(.age >= 19)
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["all", ["gte", ["get", "age"], 19]]
                ]
                """,
                """
                true
                """
            };

            yield return new object[]
            {
                """
                {
                  "friends": [
                    {"name": "Chris", "age": 23, "city": "Beijing"},
                    {"name": "Emily", "age": 19, "city": "Atlanta"},
                    {"name": "Kevin", "age": 19, "city": "Atlanta"},
                    {"name": "Michelle", "age": 27, "city": "Los Angeles"},
                    {"name": "Robert", "age": 45, "city": "Manhattan"}
                  ]
                }
                """,
                """
                .friends 
                | sort(.age)
                | all(.age < 45)
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["all", ["lt", ["get", "age"], 45]]
                ]
                """,
                """
                false
                """
            };
        }
    }

    private static IEnumerable<object[]> DemoData
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

[JsonConverter(typeof(AnyJsonConverter))]
[JsonQueryConverter(typeof(AnyJsonParserConverter))]
public class AnyQueryable : IJsonQueryable
{
    public IJsonQueryable SubQuery { get; }

    public AnyQueryable(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public JsonNode Query(JsonNode? data)
    {
        return data!.AsArray().Any(item => SubQuery.Query(item)!.GetValue<bool>());
    }
}

public class AnyJsonParserConverter : JsonQueryFunctionConverter<AnyQueryable>
{
    protected override AnyQueryable ReadArguments(ref JsonQueryReader reader)
    {
        IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        return new AnyQueryable(query);
    }
}

public class AnyJsonConverter : JsonFormatQueryJsonConverter<AnyQueryable>
{
    protected override AnyQueryable ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;
        
        reader.Read();

        return new AnyQueryable(query);
    }

    public override void Write(Utf8JsonWriter writer, AnyQueryable value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        
        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubQuery);
        
        writer.WriteEndArray();
    }
}

[JsonConverter(typeof(SingleQueryParameterConverter))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter<AllQueryable>))]
public class AllQueryable : IJsonQueryable, ISingleSubQuery
{
    private readonly IJsonQueryable _query;

    public AllQueryable(IJsonQueryable query)
    {
        _query = query;
    }

    public JsonNode Query(JsonNode? data)
    {
        return data!.AsArray().All(item => _query.Query(item)!.GetValue<bool>());
    }

    public IJsonQueryable SubQuery => _query;
}