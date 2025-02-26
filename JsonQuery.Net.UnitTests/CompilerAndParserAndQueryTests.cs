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

    public static IEnumerable<object[]> TestData => DemoData.Concat(CustomFunctionData).Concat(LinqData);

    private static IEnumerable<object[]> LinqData
    {
        get
        {
            yield return new object[]
            {
                """
                {
                  "friends": [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                }
                """,
                """
                .friends 
                | select(.age)
                | aggregate(.0 + .1)
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["select", ["get", "age"]],
                  ["aggregate", ["add", ["get", 0], ["get", 1]]]
                ]
                """,
                """
                6
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                all(.age > 0)
                """,
                """
                  ["all", ["gt", ["get", "age"], 0]]
                """,
                """
                true
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                any(.age > 2)
                """,
                """
                  ["any", ["gt", ["get", "age"], 2]]
                """,
                """
                true
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1],
                  "b": 2
                }
                """,
                """
                append(.a, .b)
                """,
                """
                  ["append", ["get", "a"], ["get", "b"]]
                """,
                """
                [1, 2]
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1],
                  "b": [2]
                }
                """,
                """
                concat(.a, .b)
                """,
                """
                  ["concat", ["get", "a"], ["get", "b"]]
                """,
                """
                [1, 2]
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                contains(.age > 2)
                """,
                """
                  ["contains", ["gt", ["get", "age"], 2]]
                """,
                """
                true
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                count(.age > 1)
                """,
                """
                  ["count", ["gt", ["get", "age"], 1]]
                """,
                """
                2
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 1]
                """,
                """
                distinct()
                """,
                """
                  ["distinct"]
                """,
                """
                [1, 2]
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                elementAtOrDefault(1)
                """,
                """
                  ["elementAtOrDefault", 1]
                """,
                """
                2
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                elementAt(1)
                """,
                """
                  ["elementAt", 1]
                """,
                """
                2
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1, 2, 3],
                  "b": [1, 3]
                }
                """,
                """
                except(.a, .b)
                """,
                """
                  ["except", ["get", "a"], ["get", "b"]]
                """,
                """
                [2]
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                firstOrDefault(get() > 1)
                """,
                """
                  ["firstOrDefault", ["gt", ["get"], 1]]
                """,
                """
                2
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                first(get() > 1)
                """,
                """
                  ["first", ["gt", ["get"], 1]]
                """,
                """
                2
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                groupByLinq(.city, .name)
                """,
                """
                  ["groupByLinq", ["get", "city"], ["get", "name"]]
                """,
                """
                [
                  {"key": "New York", "value": ["Chris"] },
                  {"key": "Atlanta", "value": ["Emily", "Kevin"] }
                ]
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1, 2, 3],
                  "b": [2, 3, 4]
                }
                
                """,
                """
                intersect(.a, .b)
                """,
                """
                  ["intersect", ["get", "a"], ["get", "b"]]
                """,
                """
                [2, 3]
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [
                    {"name": "Chris", "age": 1},
                    {"name": "Emily", "age": 2},
                    {"name": "Kevin", "age": 3}
                  ],
                  "b": [
                    {"name": "Chris", "city": "New York"},
                    {"name": "Emily", "city": "Atlanta"},
                    {"name": "Kevin", "city": "Atlanta"}
                  ]
                }
                  
                """,
                """
                joinLinq(.a, .b, .name, .name, [.0.age, .1.city])
                """,
                """
                  ["joinLinq", ["get", "a"], ["get", "b"], ["get", "name"], ["get", "name"], ["array", ["get", 0, "age"], ["get", 1, "city"]]]
                """,
                """
                [
                  [1, "New York"],
                  [2, "Atlanta"],
                  [3, "Atlanta"]
                ]
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                lastOrDefault(.city == "Atlanta")
                """,
                """
                  ["lastOrDefault", ["eq", ["get", "city"], "Atlanta"]]
                """,
                """
                {"name": "Kevin", "age": 3, "city": "Atlanta"}
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                last(.city == "Atlanta")
                """,
                """
                  ["last", ["eq", ["get", "city"], "Atlanta"]]
                """,
                """
                {"name": "Kevin", "age": 3, "city": "Atlanta"}
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                orderByDescending(.age)
                """,
                """
                  ["orderByDescending", ["get", "age"]]
                """,
                """
                [
                    {"name": "Kevin", "age": 3, "city": "Atlanta"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Chris", "age": 1, "city": "New York"} 
                ]
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                orderBy(.age)
                """,
                """
                  ["orderBy", ["get", "age"]]
                """,
                """
                [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                ]
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1, 2, 3],
                  "b": "hello"
                }
                
                """,
                """
                prepend(.a, .b)
                """,
                """
                  ["prepend", ["get", "a"], ["get", "b"]]
                """,
                """
                ["hello", 1, 2, 3]
                """
            };

            yield return new object[]
            {
                """
                [
                  {"a": [1, 2, 3]},
                  {"a": [4, 5, 6]}
                ]
                """,
                """
                selectMany(.a)
                """,
                """
                  ["selectMany", ["get", "a"]]
                """,
                """
                [1, 2, 3, 4, 5, 6]
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                select(.age)
                """,
                """
                  ["select", ["get", "age"]]
                """,
                """
                [1, 2, 3]
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1, 2, 3],
                  "b": [3, 2, 1]
                }
                
                """,
                """
                sequenceEqual(.a, .b)
                """,
                """
                  ["sequenceEqual", ["get", "a"], ["get", "b"]]
                """,
                """
                false
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1, 2, 3],
                  "b": [1, 2, 3]
                }
                
                """,
                """
                sequenceEqual(.a, .b)
                """,
                """
                  ["sequenceEqual", ["get", "a"], ["get", "b"]]
                """,
                """
                true
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                skipLast(1)
                """,
                """
                  ["skipLast", 1]
                """,
                """
                [1, 2]
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                skip(1)
                """,
                """
                  ["skip", 1]
                """,
                """
                [2, 3]
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                skipWhile(get() < 2)
                """,
                """
                  ["skipWhile", ["lt", ["get"], 2]]
                """,
                """
                [2, 3]
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                takeLast(2)
                """,
                """
                  ["takeLast", 2]
                """,
                """
                [2, 3]
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                take(2)
                """,
                """
                  ["take", 2]
                """,
                """
                [1, 2]
                """
            };

            yield return new object[]
            {
                """
                [1, 2, 3]
                """,
                """
                takeWhile(get() < 2)
                """,
                """
                  ["takeWhile", ["lt", ["get"], 2]]
                """,
                """
                [1]
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1, 2, 3],
                  "b": [2, 3, 4]
                }
                
                """,
                """
                union(.a, .b)
                """,
                """
                  ["union", ["get", "a"], ["get", "b"]]
                """,
                """
                [1, 2, 3, 4]
                """
            };

            yield return new object[]
            {
                """
                  [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"},
                    {"name": "Kevin", "age": 3, "city": "Atlanta"}
                  ]
                """,
                """
                where(.age < 3)
                """,
                """
                  ["where", ["lt", ["get", "age"], 3]]
                """,
                """
                [
                    {"name": "Chris", "age": 1, "city": "New York"},
                    {"name": "Emily", "age": 2, "city": "Atlanta"}
                ]
                """
            };

            yield return new object[]
            {
                """
                {
                  "a": [1, 2, 3, 4],
                  "b": [2, 3, 4]
                }
                
                """,
                """
                zip(.a, .b, .0 + .1)
                """,
                """
                  ["zip", ["get", "a"], ["get", "b"], ["add", ["get", 0], ["get", 1]]]
                """,
                """
                [3, 5, 7]
                """
            };
        }
    }

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
                | anyTest(.city == "New York")
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["anyTest", ["eq", ["get", "city"], "New York"]]
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
                | anyTest(.city == "New York")
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["anyTest", ["eq", ["get", "city"], "New York"]]
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
                | allTest(.age >= 19)
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["allTest", ["gte", ["get", "age"], 19]]
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
                | allTest(.age < 45)
                """,
                """
                [
                  "pipe",
                  ["get", "friends"],
                  ["sort", ["get", "age"]],
                  ["allTest", ["lt", ["get", "age"], 45]]
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

[JsonConverter(typeof(AnyTestJsonConverter))]
[JsonQueryConverter(typeof(AnyTestJsonParserConverter))]
public class AnyTestQueryable : IJsonQueryable
{
    public IJsonQueryable SubQuery { get; }

    public AnyTestQueryable(IJsonQueryable query)
    {
        SubQuery = query;
    }

    public JsonNode Query(JsonNode? data)
    {
        return data!.AsArray().Any(item => SubQuery.Query(item)!.GetValue<bool>());
    }
}

public class AnyTestJsonParserConverter : JsonQueryFunctionConverter<AnyTestQueryable>
{
    protected override AnyTestQueryable ReadArguments(ref JsonQueryReader reader)
    {
        IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

        reader.Read();

        return new AnyTestQueryable(query);
    }
}

public class AnyTestJsonConverter : JsonFormatQueryJsonConverter<AnyTestQueryable>
{
    protected override AnyTestQueryable ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader, options)!;
        
        reader.Read();

        return new AnyTestQueryable(query);
    }

    public override void Write(Utf8JsonWriter writer, AnyTestQueryable value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        
        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.SubQuery);
        
        writer.WriteEndArray();
    }
}

[JsonConverter(typeof(SingleQueryParameterConverter))]
[JsonQueryConverter(typeof(SingleQueryParameterParserConverter))]
public class AllTestQueryable : IJsonQueryable, ISingleSubQuery
{
    private readonly IJsonQueryable _query;

    public AllTestQueryable(IJsonQueryable query)
    {
        _query = query;
    }

    public JsonNode Query(JsonNode? data)
    {
        return data!.AsArray().All(item => _query.Query(item)!.GetValue<bool>());
    }

    public IJsonQueryable SubQuery => _query;
}