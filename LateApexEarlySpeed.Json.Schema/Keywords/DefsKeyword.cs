using System;
using System.Collections.Generic;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

/// <summary>
/// Reference (usage) of keyword '$defs' are not specified whether it should be json pointer encoded or not in json schema doc,
/// so this library defines it should be json pointer encoded
/// Update about above specification: Defs ref should be treated as same as normal json pointer path, so it must be json pointer encoded.
/// </summary>
internal class DefsKeyword : ISchemaContainerElement
{
    public const string Keyword = "$defs";

    private readonly Dictionary<string, JsonSchema> _definitions;

    /// <param name="definitions">Keys of it are short def name which is unescaped content</param>
    public DefsKeyword(Dictionary<string, JsonSchema> definitions)
    {
        _definitions = definitions;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return _definitions.GetValueOrDefault(name);
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return _definitions.Values;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}