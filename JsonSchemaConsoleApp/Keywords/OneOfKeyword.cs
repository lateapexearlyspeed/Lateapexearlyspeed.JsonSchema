using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("oneOf")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<OneOfKeyword>))]
internal class OneOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement
{
    public List<JsonSchema> SubSchemas { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        bool foundValidatedSchema = false;

        foreach (JsonSchema subSchema in SubSchemas)
        {
            ValidationResult result = subSchema.Validate(instance, options);
            if (result.IsValid)
            {
                if (foundValidatedSchema)
                {
                    return ValidationResult.CreateFailedResult(ResultCode.MoreThanOnePassedSchemaFound, options.ValidationPathStack);
                }

                foundValidatedSchema = true;
            }
        }

        return foundValidatedSchema 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.AllSubSchemaFailed, options.ValidationPathStack);
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return ((ISubSchemaCollection)this).GetSubElement(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return ((ISubSchemaCollection)this).EnumerateElements();
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}