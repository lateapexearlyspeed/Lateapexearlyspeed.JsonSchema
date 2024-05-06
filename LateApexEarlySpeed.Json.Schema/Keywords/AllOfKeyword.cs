using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

[Keyword(Keyword)]
[JsonConverter(typeof(SubSchemaCollectionJsonConverter<AllOfKeyword>))]
internal class AllOfKeyword : KeywordBase, ISubSchemaCollection, ISchemaContainerElement
{
    public const string Keyword = "allOf";

    private readonly List<JsonSchema> _subSchemas = null!;

    public AllOfKeyword()
    {
    }

    public AllOfKeyword(List<JsonSchema> subSchemas)
    {
        SubSchemas = subSchemas;
    }

    public List<JsonSchema> SubSchemas
    {
        get => _subSchemas;
        
        init
        {
            _subSchemas = value;
            for (int i = 0; i < _subSchemas.Count; i++)
            {
                _subSchemas[i].Name = i.ToString();
            }
        }
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
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