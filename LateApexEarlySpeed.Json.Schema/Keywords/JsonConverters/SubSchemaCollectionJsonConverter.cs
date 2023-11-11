using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JSchema;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class SubSchemaCollectionJsonConverter<T> : JsonConverter<T> where T : KeywordBase, ISubSchemaCollection, new()
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<T>(JsonValueKind.Array);
        }

        reader.Read();

        var subSchemas = new List<JsonSchema>();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            JsonSchema? subSchema = JsonSerializer.Deserialize<JsonSchema>(ref reader);

            Debug.Assert(subSchema is not null);

            subSchema.Name = subSchemas.Count.ToString();
            subSchemas.Add(subSchema);
            reader.Read();
        }

        if (subSchemas.Count == 0)
        {
            throw ThrowHelper.CreateKeywordHasEmptyJsonArrayJsonException<T>();
        }

        return new T { SubSchemas = subSchemas };
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}