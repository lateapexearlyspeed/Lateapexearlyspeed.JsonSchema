using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class ParameterlessQueryConverter : JsonConverterFactory
{
    internal static bool CanConvertInternal(Type typeToConvert)
    {
        return typeof(IJsonQueryable).IsAssignableFrom(typeToConvert)
               && typeToConvert.GetConstructor(Type.EmptyTypes) is not null;
    }

    public override bool CanConvert(Type typeToConvert) => CanConvertInternal(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(ParameterlessQueryConverterInner<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private class ParameterlessQueryConverterInner<TQuery> : JsonFormatQueryJsonConverter<TQuery> where TQuery : IJsonQueryable, new()
    {
        protected override TQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new TQuery();
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.GetKeyword());
            writer.WriteEndArray();
        }
    }
}

public class ParameterlessQueryParserConverter : IJsonQueryConverterFactory
{
    public bool CanConvert(Type typeToConvert) => ParameterlessQueryConverter.CanConvertInternal(typeToConvert);

    public IJsonQueryConverter CreateConverter(Type typeToConvert)
    {
        Type converterType = typeof(ParameterlessQueryParserConverterInner<>).MakeGenericType(typeToConvert);

        return (IJsonQueryConverter)Activator.CreateInstance(converterType);
    }

    private class ParameterlessQueryParserConverterInner<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable, new()
    {
        protected override TQuery ReadArguments(ref JsonQueryReader reader)
        {
            return new TQuery();
        }
    }
}

