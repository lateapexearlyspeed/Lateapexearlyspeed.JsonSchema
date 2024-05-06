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
internal class DefsKeyword : ISchemaContainerElement
{
    public const string Keyword = "$defs";

    public Dictionary<string, JsonSchema> Definitions { get; }

    /// <param name="definitions">Keys of it are short def name which is unescaped content</param>
    public DefsKeyword(Dictionary<string, JsonSchema> definitions)
    {
        Definitions = definitions;
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
}