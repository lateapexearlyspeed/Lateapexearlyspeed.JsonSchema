using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;
using LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

[JsonConverter(typeof(JsonSchemaJsonConverter<JsonSchema>))]
public abstract class JsonSchema : NamedValidationNode, ISchemaContainerElement
{
    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return this;
    }

    public abstract ISchemaContainerElement? GetSubElement(string name);

    public abstract IEnumerable<ISchemaContainerElement> EnumerateElements();
}