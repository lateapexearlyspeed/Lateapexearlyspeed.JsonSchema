using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

/// <summary>
/// Reference (usage) of keyword '$defs' are not specified whether it should be json pointer encoded or not in json schema doc,
/// so this library defines it should be json pointer encoded
/// Update about above specification: Defs ref should be treated as same as normal json pointer path, so it must be json pointer encoded.
/// </summary>
[JsonConverter(typeof(DefsKeywordJsonConverter))]
internal class DefsKeyword : ISchemaContainerElement, IJsonSchemaResourceNodesCleanable
{
    public const string Keyword = "$defs";
    public const string KeywordDraft7 = "definitions";

    private readonly Dictionary<string, JsonSchema> _definitions;

    public IReadOnlyDictionary<string, JsonSchema> Definitions => _definitions;

    /// <param name="definitions">Keys of it are short def name which is unescaped content</param>
    public DefsKeyword(IDictionary<string, JsonSchema> definitions)
    {
        _definitions = new Dictionary<string, JsonSchema>(definitions);
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return Definitions.GetValueOrDefault(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return Definitions.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        foreach ((string name, JsonSchema jsonSchema) in _definitions)
        {
            if (jsonSchema is BodyJsonSchema bodyJsonSchema)
            {
                BodyJsonSchema.RemoveIdForBodyJsonSchemaTree(bodyJsonSchema, newSchema => _definitions[name] = newSchema);
            }
        }
    }
}