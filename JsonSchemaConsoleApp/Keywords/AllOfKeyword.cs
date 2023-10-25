using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp.Keywords;

[Keyword("allOf")]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<AllOfKeyword>))]
public class AllOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement
{
    public List<JsonSchema> SubSchemas { get; init; } = null!;

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        foreach (JsonSchema subSchema in SubSchemas)
        {
            ValidationResult result = subSchema.Validate(instance, options);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return ValidationResult.ValidResult;
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