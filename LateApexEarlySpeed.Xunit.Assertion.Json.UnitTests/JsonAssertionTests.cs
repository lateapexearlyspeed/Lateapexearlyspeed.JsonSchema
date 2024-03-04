namespace LateApexEarlySpeed.Xunit.Assertion.Json.UnitTests
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
        public void Meet_InvalidData_Throw()
        {
            JsonAssertException jsonAssertException = Assert.Throws<JsonAssertException>(() =>
            {
                JsonAssertion.Meet(b =>
                        b.IsJsonObject()
                            .HasProperty("p1")
                            .HasProperty("p2", b => b.IsJsonNumber().Equal(5)),
                    """
                {
                  "p1": null,
                  "p2": 4.9
                }
                """);
            });

            Assert.Equal("JsonAssertion.Meet() Failure: Number not same, one is '5' but another is '4.9', location (in json pointer format): '/p2'", jsonAssertException.Message);
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

            Assert.Equal("JsonAssertion.Equivalent() Failure: Number not same, one is '2' but another is '3', location (in json pointer format): '/b'", jsonAssertException.Message);
        }
    }
}