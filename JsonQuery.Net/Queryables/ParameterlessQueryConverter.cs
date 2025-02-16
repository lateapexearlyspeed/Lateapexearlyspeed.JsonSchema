using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class ParameterlessQueryConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        ConstructorInfo? constructorInfo = typeToConvert.GetConstructor(Type.EmptyTypes);

        return constructorInfo is not null;
    }

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

public class ParameterlessQueryParserConverter<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable, new()
{
    protected override TQuery ReadArguments(ref JsonQueryReader reader)
    {
        return new TQuery();
    }
}