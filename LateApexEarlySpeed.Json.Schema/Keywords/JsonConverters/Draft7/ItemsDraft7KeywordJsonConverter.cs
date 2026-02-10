using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Draft7;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters.Draft7;

internal class ItemsDraft7KeywordJsonConverter : JsonConverter<ItemsDraft7Keyword>
{
    public override ItemsDraft7Keyword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.StartArray 
            ? JsonSerializer.Deserialize<ItemsWithMultiSchemasKeyword>(ref reader, options) 
            : JsonSerializer.Deserialize<ItemsWithOneSchemaKeyword>(ref reader, options);
    }

    /// <summary>
    /// Because the serialization target type is actual keyword types, so the serialization implementation is in the derived classes' converters
    /// </summary>
    /// <exception cref="NotImplementedException"><see cref="ItemsDraft7KeywordJsonConverter"/> not implement serialization</exception>
    public override void Write(Utf8JsonWriter writer, ItemsDraft7Keyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}