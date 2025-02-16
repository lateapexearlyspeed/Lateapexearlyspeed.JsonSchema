﻿using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class QueryCollectionConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        ConstructorInfo? constructorInfo = typeToConvert.GetConstructor(new[] { typeof(IJsonQueryable[])});

        return constructorInfo is not null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(QueryCollectionConverterInner<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType);
    }

    private class QueryCollectionConverterInner<TQuery> : JsonFormatQueryJsonConverter<TQuery> where TQuery : IJsonQueryable, IMultipleSubQuery
    {
        protected override TQuery ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var queries = new List<IJsonQueryable>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                queries.Add(JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!);

                reader.Read();
            }

            return (TQuery)Activator.CreateInstance(typeof(TQuery), new object[] { queries.ToArray() });
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            foreach (IJsonQueryable subQuery in value.SubQueries)
            {
                JsonSerializer.Serialize(writer, subQuery);
            }

            writer.WriteEndArray();
        }
    }
}

public class QueryCollectionParserConverter<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable
{
    protected override TQuery ReadArguments(ref JsonQueryReader reader)
    {
        var queries = new List<IJsonQueryable>();
        while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
        {
            queries.Add(JsonQueryParser.ParseQueryCombination(ref reader));
            reader.Read();
        }

        return (TQuery)Activator.CreateInstance(typeof(TQuery), new object[] { queries.ToArray() });
    }
}
