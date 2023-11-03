using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;

namespace JsonSchemaConsoleApp;

internal class BodyJsonSchema : JsonSchema
{
    private readonly List<KeywordBase> _keywords;

    private readonly List<ISchemaContainerValidationNode> _schemaContainerValidators;

    public SchemaReference? SchemaReference { get; }
    
    public SchemaDynamicReference? SchemaDynamicReference { get; }

    public string? Anchor { get; }

    public string? DynamicAnchor { get; }

    public BodyJsonSchema(List<KeywordBase> keywords, List<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReference? schemaReference, SchemaDynamicReference? schemaDynamicReference, string? anchor, string? dynamicAnchor)
    {
        _keywords = keywords;
        _schemaContainerValidators = schemaContainerValidators;

        SchemaReference = schemaReference;
        SchemaDynamicReference = schemaDynamicReference;
        Anchor = anchor;
        DynamicAnchor = dynamicAnchor;
    }

    protected internal override ValidationResult ValidateCore(JsonElement instance, JsonSchemaOptions options)
    {
        if (SchemaReference is not null)
        {
            ValidationResult result = SchemaReference.Validate(instance, options);
            if (!result.IsValid)
            {
                return result;
            }
        }

        if (SchemaDynamicReference is not null)
        {
            ValidationResult result = SchemaDynamicReference.Validate(instance, options);
            if (!result.IsValid)
            {
                return result;
            }
        }

        foreach (IValidationNode validationNode in _keywords.Concat<IValidationNode>(_schemaContainerValidators))
        {
            ValidationResult result = validationNode.Validate(instance, options);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return ValidationResult.ValidResult;
    }

    public override ISchemaContainerElement? GetSubElement(string name)
    {
        foreach (KeywordBase keyword in _keywords)
        {
            if (keyword.Name == name && keyword is ISchemaContainerElement schemaContainerElement)
            {
                return schemaContainerElement;
            }
        }

        foreach (ISchemaContainerValidationNode schemaContainer in _schemaContainerValidators)
        {
            ISchemaContainerElement? schemaContainerElement = schemaContainer.GetSubElement(name);
            if (schemaContainerElement is not null)
            {
                return schemaContainerElement;
            }
        }

        return null;
    }

    public override IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        foreach (KeywordBase validationKeyword in _keywords)
        {
            if (validationKeyword is ISchemaContainerElement element)
            {
                yield return element;
            }
        }

        foreach (ISchemaContainerValidationNode schemaContainer in _schemaContainerValidators)
        {
            foreach (ISchemaContainerElement element in schemaContainer.EnumerateElements())
            {
                yield return element;
            }
        }
    }
}