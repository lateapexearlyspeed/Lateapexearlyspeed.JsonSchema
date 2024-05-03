using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.FluentGenerator.ExtendedKeywords.JsonConverters;

internal class ExtendedKeywordJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(KeywordBase).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(CustomValidationKeywordJsonConverter1Inner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private class CustomValidationKeywordJsonConverter1Inner<TKeyword> : JsonConverter<TKeyword> where TKeyword : KeywordBase
    {
        public override TKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TKeyword value, JsonSerializerOptions options)
        {
            throw ThrowHelper.CreateExtendedKeywordCannotSerializeToStandardJsonSchemaException<TKeyword>();
        }
    }
}