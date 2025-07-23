using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

[JsonConverter(typeof(JsonArrayPotentialSchemaContainerElementJsonConverter))]
internal class JsonArrayPotentialSchemaContainerElement : ISchemaContainerElement
{
    private readonly IReadOnlyList<ISchemaContainerElement> _potentialSchemaElements;

    public JsonArrayPotentialSchemaContainerElement(IReadOnlyList<ISchemaContainerElement> potentialSchemaElements)
    {
        _potentialSchemaElements = potentialSchemaElements;
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return uint.TryParse(name, out uint idx) && idx < _potentialSchemaElements.Count
            ? _potentialSchemaElements[(int)idx] 
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return _potentialSchemaElements;
    }

    public bool IsSchemaType => false;

    public IReadOnlyList<ISchemaContainerElement> PotentialSchemaElements => _potentialSchemaElements;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }
}