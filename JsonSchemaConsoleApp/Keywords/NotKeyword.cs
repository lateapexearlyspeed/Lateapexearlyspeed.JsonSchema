using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("not")]
[JsonConverter(typeof(NotKeywordJsonConverter))]
public class NotKeyword : KeywordBase, ISchemaContainerElement
{
    public JsonSchema SubSchema { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        return SubSchema.Validate(instance, options).IsValid 
            ? ValidationResult.CreateFailedResult(ResultCode.SubSchemaPassed, options.ValidationPathStack) 
            : ValidationResult.ValidResult;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return SubSchema.GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return SubSchema;
    }

    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return SubSchema;
    }
}