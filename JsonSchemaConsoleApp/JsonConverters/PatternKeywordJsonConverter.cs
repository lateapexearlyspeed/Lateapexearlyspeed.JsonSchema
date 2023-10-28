using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp.JsonConverters;

public class PatternKeywordJsonConverter : JsonConverter<PatternKeyword>
{
    public override PatternKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException<PatternKeyword>(JsonValueKind.String);
        }

        string patternText = reader.GetString()!;
        try
        {
            return new PatternKeyword(patternText);
        }
        catch (Exception e)
        {
            throw ThrowHelper.CreateKeywordHasInvalidRegexJsonException<PatternKeyword>(e);
        }
    }

    public override void Write(Utf8JsonWriter writer, PatternKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}