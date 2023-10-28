using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("anyOf")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<AnyOfKeyword>))]
internal class AnyOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement
{
    public List<JsonSchema> SubSchemas { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        ValidationResult result = ValidationResult.ValidResult;

        foreach (JsonSchema subSchema in SubSchemas)
        {
            result = subSchema.Validate(instance, options);
            if (result.IsValid)
            {
                return result;
            }
        }

        return result;
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