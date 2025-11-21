using LateApexEarlySpeed.Json.Schema.Common;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class ValidationErrorTests
{
    [Fact]
    public void Validate_ValidationError_ToString()
    {
        ValidationResult validationResult = new JsonValidator("""{"type": "string"}""").Validate("1");

        ValidationError validationError = validationResult.ValidationErrors.First();

        Assert.Equal("Expect type(s): 'String' but actual is 'Number'" + Environment.NewLine 
                     + "Instance location (in json pointer format): " + Environment.NewLine
                     + "relative keyword location (in json pointer format): /type" + Environment.NewLine
                     + "keyword: type", 
            validationError.ToString());
    }

    [Theory]
    [MemberData(nameof(ToStringTestData))]
    public void TestToString(ValidationError validationError, string expectedToStringMessage)
    {
        Assert.Equal(expectedToStringMessage, validationError.ToString());
    }

    public static IEnumerable<object[]> ToStringTestData
    {
        get
        {
            const string errorMessage = "error message";
            const string keyword = "keyword-value";
            var instanceLocation = LinkedListBasedImmutableJsonPointer.Create("/abc")!;
            var keywordLocation = LinkedListBasedImmutableJsonPointer.Create("/type");

            const string expectedInstanceLocation = "Instance location (in json pointer format): /abc";
            const string expectedKeyword = "keyword: keyword-value";
            const string expectedKeywordLocation = "relative keyword location (in json pointer format): /type";

            yield return new object[] { new ValidationError(ResultCode.AllSubSchemaFailed, errorMessage, null, null, instanceLocation), 
                errorMessage + Environment.NewLine + expectedInstanceLocation + Environment.NewLine};

            yield return new object[] { new ValidationError(ResultCode.AllSubSchemaFailed, errorMessage, null, keyword, instanceLocation), 
                errorMessage + Environment.NewLine + expectedInstanceLocation + Environment.NewLine + expectedKeyword};

            yield return new object[] { new ValidationError(ResultCode.AllSubSchemaFailed, null, errorMessage, instanceLocation, null, null, null), 
                errorMessage + Environment.NewLine + expectedInstanceLocation + Environment.NewLine};

            yield return new object[] { new ValidationError(ResultCode.AllSubSchemaFailed, keyword, errorMessage, instanceLocation, null, null, null), 
                errorMessage + Environment.NewLine + expectedInstanceLocation + Environment.NewLine + expectedKeyword};

            yield return new object[] { new ValidationError(ResultCode.AllSubSchemaFailed, null, errorMessage, instanceLocation, keywordLocation, null, null), 
                errorMessage + Environment.NewLine + expectedInstanceLocation + Environment.NewLine + expectedKeywordLocation + Environment.NewLine};

            yield return new object[] { new ValidationError(ResultCode.AllSubSchemaFailed, keyword, errorMessage, instanceLocation, keywordLocation, null, null), 
                errorMessage + Environment.NewLine + expectedInstanceLocation + Environment.NewLine + expectedKeywordLocation + Environment.NewLine + expectedKeyword};
        }
    }
}