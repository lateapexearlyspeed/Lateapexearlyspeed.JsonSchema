using LateApexEarlySpeed.Json.Schema.Common;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class JsonValidatorTest_ErrorReport
{
    [Fact]
    public void Validate_CommonKeywordsValidateFail_ReportExpectedInfo()
    {
        string schema = File.ReadAllText(Path.Combine("TestData", "schema.json"));
        string instance = File.ReadAllText(Path.Combine("TestData", "instance.json"));

        var jsonValidator = new JsonValidator(schema);
        ValidationResult validationResult = jsonValidator.Validate(instance);

        Assert.False(validationResult.IsValid);
        Assert.Equal(ResultCode.InvalidTokenKind, validationResult.ResultCode);
        Assert.Equal("Expect type 'Integer' but actual is 'String'", validationResult.ErrorMessage);
        Assert.Equal("type", validationResult.Keyword);
        Assert.Equal(ImmutableJsonPointer.Create("/propArray/4"), validationResult.InstanceLocation);
        Assert.Equal(ImmutableJsonPointer.Create("/properties/propArray/items/type"), validationResult.RelativeKeywordLocation);
        Assert.Equal(new Uri("http://main"), validationResult.SchemaResourceBaseUri);
        Assert.Equal(new Uri("http://main"), validationResult.SubSchemaRefFullUri);
    }
}