using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Xunit.V3.Assertion.Json;

namespace LateApexEarlySpeed.V3.Assertion.Json.UnitTests
{
    public class JsonAssertionTests
    {
        [Fact]
        public void Meet_ValidData()
        {
            JsonAssertion.Meet(b =>
                b.IsJsonObject()
                .HasProperty("p1")
                .HasProperty("p2", b => b.IsJsonNumber().Equal(5)),
                """
                {
                  "p1": null,
                  "p2": 5
                }
                """);
        }

        [Fact]
        public void Meet_InvalidArrayData_Throw()
        {
            JsonAssertException jsonAssertException = Assert.Throws<JsonAssertException>(() =>
            {
                JsonAssertion.Meet(b =>
                    b.IsJsonArray()
                        .Contains(b => b.IsJsonTrue())
                        .HasCollection(b => b.IsJsonTrue(), b => b.IsJsonFalse())
                        .HasItems(b => b.IsJsonNull()), "[1, 2]");
            });

            string expectedExceptionMessage = "JsonAssertion.Meet() Failure: "
                                              + "Expect type(s): 'Null' but actual is 'Number', location (in json pointer format): \"/0\"" + Environment.NewLine
                                              + "Expect type(s): 'Null' but actual is 'Number', location (in json pointer format): \"/1\"" + Environment.NewLine
                                              + "Not found any validated array items, array instance: [1, 2], location (in json pointer format): \"\"" + Environment.NewLine
                                              + "Json kind not same, one is True, but another is Number, location (in json pointer format): \"/0\"" + Environment.NewLine
                                              + "Json kind not same, one is True, but another is Number, location (in json pointer format): \"/1\"" + Environment.NewLine
                                              + "Json kind not same, one is True, but another is Number, location (in json pointer format): \"/0\"" + Environment.NewLine
                                              + "Json kind not same, one is False, but another is Number, location (in json pointer format): \"/1\"" + Environment.NewLine;

            Assert.Equal(expectedExceptionMessage, jsonAssertException.Message);
        }

        [Fact]
        public void Meet_InvalidObjectData_Throw()
        {
            JsonAssertException jsonAssertException = Assert.Throws<JsonAssertException>(() =>
            {
                JsonAssertion.Meet(b =>
                        b.IsJsonObject()
                            .HasProperty("p1")
                            .HasProperty("p2", b => b.IsJsonNumber().Equal(5)),
                    """
                {
                  "p2": 4.9
                }
                """);
            });

            Assert.Equal("JsonAssertion.Meet() Failure: Number not same, one is '5' but another is '4.9', location (in json pointer format): \"/p2\"" + Environment.NewLine
                       + "Instance not contain required property 'p1', location (in json pointer format): \"\"" + Environment.NewLine, jsonAssertException.Message);
        }

        [Fact]
        public void Equivalent_ValidData()
        {
            JsonAssertion.Equivalent("""
                {
                  "a": 1,
                  "b": 2
                }
                """,
                """
                {
                  "b": 2,
                  "a": 1
                }
                """);
        }

        [Fact]
        public void Equivalent_InvalidData_Throw()
        {
            JsonAssertException jsonAssertException = Assert.Throws<JsonAssertException>(() =>
            {
                JsonAssertion.Equivalent("""
                {
                  "a": 1,
                  "b": 2
                }
                """,
                    """
                {
                  "a": 1,
                  "b": 3
                }
                """);
            });

            Assert.Equal("JsonAssertion.Equivalent() Failure: Number not same, one is '2' but another is '3', location (in json pointer format): \"/b\"" + Environment.NewLine, jsonAssertException.Message);
        }

        [Theory]
        [MemberData(nameof(TestDataForJsonArrayEqualityComparerOption))]
        public void Equivalent_AssertArrayWithJsonArrayEqualityComparerOption(string actualJson, JsonCollectionEqualityComparer? jsonCollectionEqualityComparer, bool expectedAssertResult)
        {
            if (expectedAssertResult)
            {
                RunAssertion();
            }
            else
            {
                Assert.Throws<JsonAssertException>(RunAssertion);
            }

            void RunAssertion()
            {
                const string expectedJson = "[1, 2]";
                JsonAssertion.Equivalent(expectedJson, actualJson, jsonCollectionEqualityComparer is null ? null : new JsonAssertionOptions { JsonArrayEqualityComparer = jsonCollectionEqualityComparer });
            }
        }

        public static IEnumerable<object?[]> TestDataForJsonArrayEqualityComparerOption
        {
            get
            {
                yield return new object?[] { "[1, 2]", null, true };
                yield return new object?[] { "[1, 2]", JsonCollectionEqualityComparer.Equality, true };
                yield return new object?[] { "[1, 2]", JsonCollectionEqualityComparer.Equivalence, true };

                yield return new object?[] { "[2, 1]", null, false };
                yield return new object?[] { "[2, 1]", JsonCollectionEqualityComparer.Equality, false };
                yield return new object?[] { "[2, 1]", JsonCollectionEqualityComparer.Equivalence, true };

                yield return new object?[] { "[1, 2, 3]", null, false };
                yield return new object?[] { "[1, 2, 3]", JsonCollectionEqualityComparer.Equality, false };
                yield return new object?[] { "[1, 2, 3]", JsonCollectionEqualityComparer.Equivalence, false };
            }
        }
    }
}