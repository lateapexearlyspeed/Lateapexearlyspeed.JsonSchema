using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

[JsonConverter(typeof(JsonArrayPotentialSchemaContainerElementJsonConverter))]
internal class JsonArrayPotentialSchemaContainerElement : ISchemaContainerElement, IJsonSchemaResourceNodesCleanable
{
    private readonly ISchemaContainerElement[] _potentialSchemaElements;

    public JsonArrayPotentialSchemaContainerElement(IEnumerable<ISchemaContainerElement> potentialSchemaElements)
    {
        _potentialSchemaElements = potentialSchemaElements.ToArray();
    }

    public ISchemaContainerElement? GetSubElement(string name)
    {
        return uint.TryParse(name, out uint idx) && idx < _potentialSchemaElements.Length
            ? _potentialSchemaElements[idx] 
            : null;
    }

    public IEnumerable<ISchemaContainerElement> EnumerateElements()
    {
        return _potentialSchemaElements;
    }

    public bool IsSchemaType => false;

    public IEnumerable<ISchemaContainerElement> PotentialSchemaElements => _potentialSchemaElements;

    public JsonSchema GetSchema()
    {
        throw new InvalidOperationException();
    }

    public void RemoveIdFromAllChildrenSchemaElements()
    {
        for (int i = 0; i < _potentialSchemaElements.Length; i++)
        {
            if (_potentialSchemaElements[i] is IJsonSchemaResourceNodesCleanable jsonSchemaResourceNodesCleanable)
            {
                jsonSchemaResourceNodesCleanable.RemoveIdFromAllChildrenSchemaElements();

                if (_potentialSchemaElements[i] is JsonSchemaResource jsonSchemaResource)
                {
                    BodyJsonSchema newSchema = jsonSchemaResource.TransformToBodyJsonSchema();
                    _potentialSchemaElements[i] = newSchema;
                }
            }
        }
    }
}