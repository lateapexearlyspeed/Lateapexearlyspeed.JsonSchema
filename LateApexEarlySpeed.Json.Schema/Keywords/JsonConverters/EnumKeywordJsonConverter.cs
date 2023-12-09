using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class EnumKeywordJsonConverter : JsonConverter<EnumKeyword>
{
    public override EnumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<EnumKeyword>(JsonValueKind.Array);
        }

        reader.Read();

        var enums = new List<JsonInstanceElement>();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            JsonInstanceElement item = JsonInstanceElement.ParseValue(ref reader);

            if (enums.Contains(item))
            {
                throw ThrowHelper.CreateKeywordHasDuplicatedJsonArrayElementsJsonException<EnumKeyword>();
            }

            enums.Add(item);

            reader.Read();
        }

        if (enums.Count == 0)
        {
            throw ThrowHelper.CreateKeywordHasEmptyJsonArrayJsonException<EnumKeyword>();
        }

        return new EnumKeyword(enums);
    }

    public override void Write(Utf8JsonWriter writer, EnumKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}