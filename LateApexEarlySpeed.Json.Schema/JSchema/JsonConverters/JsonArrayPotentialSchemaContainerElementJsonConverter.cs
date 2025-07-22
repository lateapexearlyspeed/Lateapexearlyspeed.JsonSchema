using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common.interfaces;

namespace LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

internal class JsonArrayPotentialSchemaContainerElementJsonConverter : JsonConverter<JsonArrayPotentialSchemaContainerElement>
{
    public override JsonArrayPotentialSchemaContainerElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(reader.TokenType == JsonTokenType.StartArray);

        reader.Read();

        var elements = new List<ISchemaContainerElement>();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (PotentialSchemaContainerElement.TryDeserialize(ref reader, out ISchemaContainerElement? element, options))
            {
                elements.Add(element);
            }

            reader.Read();
        }

        return new JsonArrayPotentialSchemaContainerElement(elements);
    }



    public override void Write(Utf8JsonWriter writer, JsonArrayPotentialSchemaContainerElement value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}