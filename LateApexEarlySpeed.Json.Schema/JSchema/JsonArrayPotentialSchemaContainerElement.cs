using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

[JsonConverter(typeof(JsonArrayPotentialSchemaContainerElementJsonConverter))]
internal class JsonArrayPotentialSchemaContainerElement : ISchemaContainerElement
{
    private readonly IReadOnlyList<ISchemaContainerElement> _schemaContainerElements;

    public JsonArrayPotentialSchemaContainerElement(IReadOnlyList<ISchemaContainerElement> schemaContainerElements)
    {
        _schemaContainerElements = schemaContainerElements;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return uint.TryParse(name, out uint idx) && idx < _schemaContainerElements.Count
            ? _schemaContainerElements[(int)idx] 
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return _schemaContainerElements;
    }

    public bool IsSchemaType => false;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}