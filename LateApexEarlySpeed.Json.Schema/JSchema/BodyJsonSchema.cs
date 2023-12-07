using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal class BodyJsonSchema : JsonSchema
{
    // {
    //     "$schema": "https://json-schema.org/draft/2020-12/schema",
    //     "$id": "http://example.com/a.json",
    //     "$defs": {
    //         "x": {
    //             "$id": "http://example.com/b/c.json",
    //             "not": {
    //                 "$defs": {
    //                     "y": {
    //                         "$id": "d.json",
    //                         "type": "number"
    //                     }
    //                 }
    //             }
    //         }
    //     },
    //     "allOf": [
    //     {
    //         "$ref": "http://example.com/b/d.json"
    //     }
    //     ]
    // }
    /// <summary>
    /// Pure body json schema is also able to contain '$defs' keyword,
    /// which is ONLY used to enumerate its inner subschema or sub schema resource (with $id)
    /// without functionality of 'find by defs-ref' because pure json schema is not a schema resource (no $id)
    /// </summary>
    private readonly DefsKeyword? _defsKeyword;

    private readonly List<ISchemaContainerValidationNode> _schemaContainerValidators;

    private readonly List<KeywordBase> _keywords;

    public SchemaReferenceKeyword? SchemaReference { get; }

    public SchemaDynamicReferenceKeyword? SchemaDynamicReference { get; }

    public string? Anchor { get; }

    public string? DynamicAnchor { get; }

    public BodyJsonSchema(List<KeywordBase> keywords) : this(keywords, new List<ISchemaContainerValidationNode>(0), null, null, null, null, null)
    {

    }

    public BodyJsonSchema(List<KeywordBase> keywords, List<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReferenceKeyword? schemaReference, SchemaDynamicReferenceKeyword? schemaDynamicReference, string? anchor, string? dynamicAnchor, DefsKeyword? defsKeyword)
    {
        _keywords = keywords;

        Debug.Assert(schemaContainerValidators.All(
                validator
                    => validator.GetType() == typeof(ConditionalValidator)
                    || validator.GetType() == typeof(ArrayContainsValidator)));
        _schemaContainerValidators = schemaContainerValidators;

        SchemaReference = schemaReference;
        SchemaDynamicReference = schemaDynamicReference;
        
        _defsKeyword = defsKeyword;

        Anchor = anchor;
        DynamicAnchor = dynamicAnchor;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        IEnumerable<IValidationNode> validationNodes = _keywords.Concat<IValidationNode>(_schemaContainerValidators);

        if (SchemaReference is not null)
        {
            validationNodes = validationNodes.Append(SchemaReference);
        }

        if (SchemaDynamicReference is not null)
        {
            validationNodes = validationNodes.Append(SchemaDynamicReference);
        }

        foreach (IValidationNode validationNode in validationNodes)
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

        if (name == DefsKeyword.Keyword)
        {
            return _defsKeyword;
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

        IEnumerable<ISchemaContainerElement> schemaContainers = _schemaContainerValidators;
        if (_defsKeyword is not null)
        {
            schemaContainers = schemaContainers.Append(_defsKeyword);
        }

        foreach (ISchemaContainerElement containerElement in schemaContainers)
        {
            yield return containerElement;
        }
    }

    public virtual Uri ParentResourceBaseUri
    {
        set
        {
            if (SchemaReference is not null)
            {
                SchemaReference.ParentResourceBaseUri = value;
            }

            if (SchemaDynamicReference is not null)
            {
                SchemaDynamicReference.ParentResourceBaseUri = value;
            }
        }
    }

    public BodyJsonSchemaDocument TransformToSchemaDocument(Uri id, DefsKeyword defsKeyword)
    {
        return new BodyJsonSchemaDocument(_keywords, _schemaContainerValidators, SchemaReference, SchemaDynamicReference, Anchor, DynamicAnchor, id, defsKeyword);
    }
}