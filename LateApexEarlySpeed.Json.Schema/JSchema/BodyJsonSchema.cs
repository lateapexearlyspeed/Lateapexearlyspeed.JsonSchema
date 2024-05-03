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
    public DefsKeyword? DefsKeyword { get; }

    public List<ISchemaContainerValidationNode> SchemaContainerValidators { get; }

    public List<KeywordBase> Keywords { get; }

    public SchemaReferenceKeyword? SchemaReference { get; }

    public SchemaDynamicReferenceKeyword? SchemaDynamicReference { get; }

    public string? Anchor { get; }

    public string? DynamicAnchor { get; }

    public BodyJsonSchema(List<KeywordBase> keywords) : this(keywords, new List<ISchemaContainerValidationNode>(0), null, null, null, null, null)
    {

    }

    public BodyJsonSchema(List<KeywordBase> keywords, List<ISchemaContainerValidationNode> schemaContainerValidators, SchemaReferenceKeyword? schemaReference, SchemaDynamicReferenceKeyword? schemaDynamicReference, string? anchor, string? dynamicAnchor, DefsKeyword? defsKeyword)
    {
        Keywords = MergeKeywords(keywords);

        Debug.Assert(schemaContainerValidators.All(
                validator
                    => validator.GetType() == typeof(ConditionalValidator)
                    || validator.GetType() == typeof(ArrayContainsValidator)));
        SchemaContainerValidators = schemaContainerValidators;

        SchemaReference = schemaReference;
        SchemaDynamicReference = schemaDynamicReference;
        
        DefsKeyword = defsKeyword;

        Anchor = anchor;
        DynamicAnchor = dynamicAnchor;
    }

    /// <summary>
    /// Put all duplicated keywords and original 'allOf' keyword(s) into one newly created 'allOf' keyword, so that de-dup them
    /// </summary>
    private static List<KeywordBase> MergeKeywords(List<KeywordBase> keywords)
    {
        if (FindFirstDuplicatedKeyword(keywords) is null)
        {
            return keywords;
        }

        var keywordGroups = new Dictionary<string, List<KeywordBase>>(keywords.Count);
        foreach (KeywordBase keyword in keywords)
        {
            if (!keywordGroups.TryGetValue(keyword.Name, out List<KeywordBase>? group))
            {
                // Assume there is no much duplication cases, so initialize 'group' with capacity of 1
                group = new List<KeywordBase>(1);
                keywordGroups[keyword.Name] = group;
            }

            group.Add(keyword);
        }

        var result = new List<KeywordBase>(keywords.Count);

        // this includes all duplicated keywords (including 'allOf' keyword)
        var duplicatedKeywords = new List<KeywordBase>();
        foreach (KeyValuePair<string, List<KeywordBase>> keywordGroup in keywordGroups)
        {
            if (keywordGroup.Key == AllOfKeyword.Keyword) // found 'allOf' keyword(s)
            {
                duplicatedKeywords.AddRange(keywordGroup.Value);
            }
            else if (keywordGroup.Value.Count > 1) // found duplicated keyword group
            {
                duplicatedKeywords.AddRange(keywordGroup.Value);
            }
            else
            {
                result.Add(keywordGroup.Value[0]);
            }
        }

        Debug.Assert(duplicatedKeywords.Count != 0);

        // Combine all duplicated keywords into ONE new 'allOf' keyword
        var allOfKeyword = new AllOfKeyword(duplicatedKeywords.Select(k => new BodyJsonSchema(new List<KeywordBase>(1) { k })).Cast<JsonSchema>().ToList());
        result.Add(allOfKeyword);

        return result;
    }

    protected internal override ValidationResult ValidateCore(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        IEnumerable<IValidationNode> validationNodes = Keywords.Concat<IValidationNode>(SchemaContainerValidators);

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
        foreach (KeywordBase keyword in Keywords)
        {
            if (keyword.Name == name && keyword is ISchemaContainerElement schemaContainerElement)
            {
                return schemaContainerElement;
            }
        }

        foreach (ISchemaContainerValidationNode schemaContainer in SchemaContainerValidators)
        {
            ISchemaContainerElement? schemaContainerElement = schemaContainer.GetSubElement(name);
            if (schemaContainerElement is not null)
            {
                return schemaContainerElement;
            }
        }

        if (name == DefsKeyword.Keyword)
        {
            return DefsKeyword;
        }

        return null;
    }

    public override IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        foreach (KeywordBase validationKeyword in Keywords)
        {
            if (validationKeyword is ISchemaContainerElement element)
            {
                yield return element;
            }
        }

        IEnumerable<ISchemaContainerElement> schemaContainers = SchemaContainerValidators;
        if (DefsKeyword is not null)
        {
            schemaContainers = schemaContainers.Append(DefsKeyword);
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
        return new BodyJsonSchemaDocument(Keywords, SchemaContainerValidators, SchemaReference, SchemaDynamicReference, Anchor, DynamicAnchor, id, defsKeyword);
    }

    public BodyJsonSchemaDocument TransformToSchemaDocument(Uri id)
    {
        return new BodyJsonSchemaDocument(Keywords, SchemaContainerValidators, SchemaReference, SchemaDynamicReference, Anchor, DynamicAnchor, id, DefsKeyword);
    }

    /// <returns>One of duplicated keyword if duplication occurs; null otherwise.</returns>
    public static KeywordBase? FindFirstDuplicatedKeyword(List<KeywordBase> keywords)
    {
        if (keywords.Count == 0 || keywords.Count == 1)
        {
            return null;
        }

        var keywordHash = new HashSet<string>(keywords.Count);
        foreach (KeywordBase keyword in keywords)
        {
            if (!keywordHash.Add(keyword.Name))
            {
                return keyword;
            }
        }

        return null;
    }
}