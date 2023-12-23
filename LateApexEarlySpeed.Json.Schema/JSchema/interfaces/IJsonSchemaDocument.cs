using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.JSchema.interfaces;

[JsonConverter(typeof(JsonSchemaJsonConverter<IJsonSchemaDocument>))]
public interface IJsonSchemaDocument
{
    ValidationResult DoValidation(JsonInstanceElement instance, JsonSchemaOptions options);
}