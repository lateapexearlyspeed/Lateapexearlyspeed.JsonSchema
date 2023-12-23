using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

[Format("custom_format")]
public class TestCustomFormatValidator : FormatValidator
{
    public override bool Validate(string content)
    {
        switch (content)
        {
            case "true":
                return true;
            case "false":
                return false;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}