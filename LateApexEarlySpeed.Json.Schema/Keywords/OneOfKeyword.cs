using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword("oneOf")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<OneOfKeyword>))]
internal class OneOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement
{
    private readonly JsonSchema[] _subSchemas = null!;

    public IReadOnlyList<JsonSchema> SubSchemas
    {
        get => _subSchemas;
        init => _subSchemas = CreateSubSchema(value);
    }

    [Pure]
    private static JsonSchema[] CreateSubSchema(IEnumerable<JsonSchema> subSchemas)
    {
        JsonSchema[] result = subSchemas.ToArray();

        for (int i = 0; i < result.Length; i++)
        {
            result[i].Name = i.ToString();
        }

        return result;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        bool foundValidatedSchema = false;

        foreach (JsonSchema subSchema in SubSchemas)
        {
            ValidationResult result = subSchema.Validate(instance, options);
            if (result.IsValid)
            {
                if (foundValidatedSchema)
                {
                    return ValidationResult.CreateFailedResult(ResultCode.MoreThanOnePassedSchemaFound, "More than one schema validate instance", options.ValidationPathStack, Name, instance.Location);
                }

                foundValidatedSchema = true;
            }
        }

        return foundValidatedSchema 
            ? ValidationResult.ValidResult 
            : ValidationResult.CreateFailedResult(ResultCode.AllSubSchemaFailed, "All schemas not validated instance", options.ValidationPathStack, Name, instance.Location);
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