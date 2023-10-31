using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSchemaConsoleApp.Keywords;

namespace JsonSchemaConsoleApp.JsonConverters;

internal class NumberRangeKeywordJsonConverter<TNumberRangeLimitKeyword> : JsonConverter<TNumberRangeLimitKeyword> where TNumberRangeLimitKeyword : NumberRangeKeywordBase, new()
{
    public override TNumberRangeLimitKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw ThrowHelper.CreateKeywordHasInvalidJsonValueKindJsonException(typeToConvert, JsonValueKind.Number);
        }

        return new TNumberRangeLimitKeyword { BenchmarkValue = reader.GetDouble() };
    }

    public override void Write(Utf8JsonWriter writer, TNumberRangeLimitKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}