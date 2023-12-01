using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class UniqueItemsKeywordJsonConverter : JsonConverter<UniqueItemsKeyword>
{
    public override UniqueItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True)
        {
            return new UniqueItemsKeyword(true);
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return new UniqueItemsKeyword(false);
        }

        throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<UniqueItemsKeyword>(JsonValueKind.True, JsonValueKind.False);
    }

    public override void Write(Utf8JsonWriter writer, UniqueItemsKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}