﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class SingleQueryParameterConverter : JsonConverterFactory
{
    internal static bool CanConvertInternal(Type typeToConvert)
    {
        return typeof(IJsonQueryable).IsAssignableFrom(typeToConvert) 
               && typeToConvert.GetConstructor(new[] { typeof(IJsonQueryable) }) is not null;
    }

    public override bool CanConvert(Type typeToConvert) => CanConvertInternal(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(SingleQueryParameterConverterInner<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private class SingleQueryParameterConverterInner<TQuery> : JsonFormatQueryJsonConverter<TQuery> where TQuery : IJsonQueryable, ISingleSubQuery
    {
        protected override TQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader, options)!;

            reader.Read();

            return (TQuery)Activator.CreateInstance(typeof(TQuery), query);
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubQuery, options);
            writer.WriteEndArray();
        }
    }
}

public class SingleQueryParameterParserConverter : IJsonQueryConverterFactory
{
    public bool CanConvert(Type typeToConvert) => SingleQueryParameterConverter.CanConvertInternal(typeToConvert);

    public IJsonQueryConverter CreateConverter(Type typeToConvert)
    {
        Type converterType = typeof(SingleQueryParameterParserConverterInner<>).MakeGenericType(typeToConvert);

        return (IJsonQueryConverter)Activator.CreateInstance(converterType);
    }

    private class SingleQueryParameterParserConverterInner<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable
    {
        protected override TQuery ReadArguments(ref JsonQueryReader reader)
        {
            IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read(); // ')'

            return (TQuery)Activator.CreateInstance(typeof(TQuery), query);
        }
    }
}
