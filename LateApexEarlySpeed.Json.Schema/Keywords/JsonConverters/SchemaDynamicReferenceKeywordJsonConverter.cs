using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class SchemaDynamicReferenceKeywordJsonConverter : JsonConverter<SchemaDynamicReferenceKeyword>
{
    public override SchemaDynamicReferenceKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<SchemaDynamicReferenceKeyword>(JsonValueKind.String);
        }

        try
        {
            return new SchemaDynamicReferenceKeyword(new Uri(reader.GetString()!, UriKind.RelativeOrAbsolute));
        }
        catch (Exception e)
        {
            throw ThrowHelper.CreateKeywordHasInvalidUriJsonException<SchemaDynamicReferenceKeyword>(e);
        }
    }

    public override void Write(Utf8JsonWriter writer, SchemaDynamicReferenceKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}