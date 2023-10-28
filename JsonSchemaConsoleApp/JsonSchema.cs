using JsonSchemaConsoleApp.JsonConverters;
using JsonSchemaConsoleApp.Keywords;
using JsonSchemaConsoleApp.Keywords.interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonSchemaConsoleApp;

[JsonConverter(typeof(JsonSchemaJsonConverter<JsonSchema>))]
internal abstract class JsonSchema : ValidationNode, ISchemaContainerElement
{
    public bool IsSchemaType => true;

    public JsonSchema GetSchema()
    {
        return this;
    }

    public abstract ISchemaContainerElement? GetSubElement(string name);

    public abstract IEnumerable<ISchemaContainerElement> EnumerateElements();
}