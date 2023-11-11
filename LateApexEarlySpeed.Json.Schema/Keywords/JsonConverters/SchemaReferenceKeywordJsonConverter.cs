using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class SchemaReferenceKeywordJsonConverter : JsonConverter<SchemaReferenceKeyword>
{
    public override SchemaReferenceKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<SchemaReferenceKeyword>(JsonValueKind.String);
        }

        try
        {
            return new SchemaReferenceKeyword(new Uri(reader.GetString()!, UriKind.RelativeOrAbsolute));
        }
        catch (Exception e)
        {
            throw ThrowHelper.CreateKeywordHasInvalidUriJsonException<SchemaReferenceKeyword>(e);
        }
    }

    public override void Write(Utf8JsonWriter writer, SchemaReferenceKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}