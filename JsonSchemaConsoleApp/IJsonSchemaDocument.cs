using System.Text.Json;
using JsonSchemaConsoleApp.JsonConverters;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp;

[JsonConverter(typeof(JsonSchemaJsonConverter<IJsonSchemaDocument>))]
public interface IJsonSchemaDocument
{
    ValidationResult Validate(JsonElement instance);
}