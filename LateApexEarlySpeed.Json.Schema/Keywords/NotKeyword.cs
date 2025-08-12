using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("not")]
[JsonConverter(typeof(SingleSchemaJsonConverter<NotKeyword>))]
internal class NotKeyword : KeywordBase, ISchemaContainerElement, ISingleSubSchema, IJsonSchemaResourceNodesCleanable
{
    private JsonSchema _schema = null!;

    public JsonSchema Schema
    {
        get => _schema;
        init => _schema = value;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        ValidationResult validationResult = Schema.Validate(instance, options);

        if (validationResult.IsValid)
        {
            var curError = new ValidationError(ResultCode.SubSchemaPassedUnexpected, ErrorMessage(instance.ToString()), options.ValidationPathStack, Name, instance.Location);

            if (options.OutputFormat == OutputFormat.FailFast)
            {
                return ValidationResult.SingleErrorFailedResult(curError);
            }

            var errorBuilder = new ImmutableValidationErrorCollection.Builder();
            errorBuilder.SetCurrent(curError);
            errorBuilder.AddChildCollection(validationResult.ValidationErrorsList);
            return new ValidationResult(false, errorBuilder.ToImmutable());
        }

        if (options.OutputFormat == OutputFormat.FailFast)
        {
            return ValidationResult.ValidResult;
        }

        return new ValidationResult(true, validationResult.ValidationErrorsList);
    }

    public static string ErrorMessage(string instanceText)
    {
        return $"Instance is validated by subSchema which is not allowed, instance data: '{instanceText}'";
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return Schema.GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        yield return Schema;
    }

    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return Schema;
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        if (_schema is BodyJsonSchema bodyJsonSchema)
        {
            BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _schema = newSchema);
        }
    }
}