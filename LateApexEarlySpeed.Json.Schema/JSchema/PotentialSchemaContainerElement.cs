using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

internal static class PotentialSchemaContainerElement
{
    public static bool TryDeserialize(ref Utf8JsonReader reader, [NotNullWhen(true)] out ISchemaContainerElement? potentialSchemaContainerElement, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
        {
            potentialSchemaContainerElement = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;
            return true;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            potentialSchemaContainerElement = JsonSerializer.Deserialize<JsonArrayPotentialSchemaContainerElement>(ref reader, options)!;
            return true;
        }

        // if current node is not valid token type to be a potential schema container element, its token type can only be 'single value' type,
        // so we don't need to advance the reader position (by reader.Skip()) here.
        potentialSchemaContainerElement = null;
        return false;
    }

    public static void Serialize(Utf8JsonWriter writer, ISchemaContainerElement schemaContainerElement, JsonSerializerOptions options)
    {
        Type inputType;

        if (schemaContainerElement is JsonSchema)
        {
            inputType = typeof(JsonSchema);
        }
        else if (schemaContainerElement is JsonArrayPotentialSchemaContainerElement)
        {
            inputType = typeof(JsonArrayPotentialSchemaContainerElement);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(schemaContainerElement), schemaContainerElement.GetType(), $"Support to serialize {nameof(JsonSchema)} and {nameof(JsonArrayPotentialSchemaContainerElement)}");
        }

        JsonSerializer.Serialize(writer, schemaContainerElement, inputType, options);
    }
}