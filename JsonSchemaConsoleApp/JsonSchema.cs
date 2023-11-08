using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords.interfaces;
using System.Text.Json.Serialization;

namespace JsonSchemaConsoleApp;

[JsonConverter(typeof(JsonSchemaJsonConverter<JsonSchema>))]
internal abstract class JsonSchema : NamedValidationNode, ISchemaContainerElement
{
    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return this;
    }

    public abstract ISchemaContainerElement? GetSubElement(string name);

    public abstract IEnumerable<ISchemaContainerElement> EnumerateElements();
}