using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

public class QueryCollectionConverter : JsonConverterFactory
{
    internal static bool CanConvertInternal(Type typeToConvert)
    {
        return typeof(IJsonQueryable).IsAssignableFrom(typeToConvert)
               && typeof(IMultipleSubQuery).IsAssignableFrom(typeToConvert)
               && typeToConvert.GetConstructor(new[] { typeof(IJsonQueryable[]) }) is not null;
    }

    public override bool CanConvert(Type typeToConvert) => CanConvertInternal(typeToConvert);

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
                queries.Add(JsonSerializer.Deserialize<IJsonQueryable>(ref reader, options)!);

                reader.Read();
            }

            try
            {
                return (TQuery)Activator.CreateInstance(typeof(TQuery), new object[] { queries.ToArray() });
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw new JsonException($"Failed to deserialize '{QueryKeyword}' query", targetInvocationException.InnerException);
            }
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            foreach (IJsonQueryable subQuery in value.SubQueries)
            {
                JsonSerializer.Serialize(writer, subQuery, options);
            }

            writer.WriteEndArray();
        }
    }
}

public class QueryCollectionParserConverter : IJsonQueryConverterFactory
{
    public bool CanConvert(Type typeToConvert) => QueryCollectionConverter.CanConvertInternal(typeToConvert);

    public IJsonQueryConverter CreateConverter(Type typeToConvert)
    {
        Type converterType = typeof(QueryCollectionParserConverterInner<>).MakeGenericType(typeToConvert);

        return (IJsonQueryConverter)Activator.CreateInstance(converterType);
    }

    private class QueryCollectionParserConverterInner<TQuery> : JsonQueryFunctionConverter<TQuery> where TQuery : IJsonQueryable
    {
        protected override TQuery ReadArguments(ref JsonQueryReader reader)
        {
            var queries = new List<IJsonQueryable>();
            while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
            {
                queries.Add(JsonQueryParser.ParseQueryCombination(ref reader));
                reader.Read();
            }

            try
            {
                return (TQuery)Activator.CreateInstance(typeof(TQuery), new object[] { queries.ToArray() });
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw new JsonQueryParseException($"Failed to parse '{QueryKeyword}' function", reader.Position, targetInvocationException.InnerException);
            }
        }
    }
}


