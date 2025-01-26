using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace JsonQuery.Net
{
    public class JsonFormatCompiler
    {
        public IJsonQueryable Compile(string jsonFormatQuery)
        {
            return JsonSerializer.Deserialize<IJsonQueryable>(jsonFormatQuery)!;
        }

        public IJsonQueryable Compile(JsonNode? jsonFormatQuery)
        {
            return jsonFormatQuery.Deserialize<IJsonQueryable>()!;
        }
    }

    public static class JsonQueryableExtensions
    {
        public static string GetKeyword(this IJsonQueryable queryable)
        {
            return JsonQueryableRegistry.GetKeyword(queryable.GetType());
        }

        public static string SerializeToJsonFormat(this IJsonQueryable queryable)
        {
            return queryable is ConstQueryable constQuery 
                ? (constQuery.Value?.ToJsonString() ?? "null") 
                : JsonSerializer.Serialize(queryable);
        }
    }

    static class JsonQueryableRegistry
    {
        private static readonly Dictionary<string, Type> JsonQueryables;
        private static readonly Dictionary<Type, string> QueryTypeKeywordsMap;

        static JsonQueryableRegistry()
        {
            JsonQueryables = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && typeof(IJsonQueryable).IsAssignableFrom(type) && type != typeof(ConstQueryable)).ToDictionary(type => (string)type.GetField("Keyword", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue());
            QueryTypeKeywordsMap = JsonQueryables.ToDictionary(kv => kv.Value, kv => kv.Key);
        }

        public static bool TryGetQueryableType(string keyword, [NotNullWhen(true)]out Type? queryType) => JsonQueryables.TryGetValue(keyword, out queryType);

        public static string GetKeyword(Type queryType)
        {
            return QueryTypeKeywordsMap[queryType];
        }
    }

    static class OperatorRegistry
    {
        private static readonly Dictionary<string, Type> Operators;

        public static string[] SortedOperatorKeywords { get; } = new[]
        {
            EqQuery.Operator,
            GteQuery.Operator,
            GtQuery.Operator,
            LteQuery.Operator,
            LtQuery.Operator,
            NeQuery.Operator,
            AndQuery.Operator,
            OrQuery.Operator,
            NotInQuery.Operator,
            InQuery.Operator,
            AddOperator.Operator,
            SubtractQuery.Operator,
            MultiplyOperator.Operator,
            DivideQuery.Operator,
            ModQuery.Operator,
            PowQuery.Operator
        };

        static OperatorRegistry()
        {
            Operators = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && typeof(OperatorQuery).IsAssignableFrom(type)).ToDictionary(type => (string)type.GetField("Operator", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue());
        }

        public static Type FindOperatorType(string operatorName) => Operators[operatorName];
    }

    [JsonConverter(typeof(QueryCollectionConverter<ArrayQuery>))]
    public class ArrayQuery : IJsonQueryable, IMultipleSubQuery
    {
        internal const string Keyword = "array";

        private readonly IJsonQueryable[] _queries;

        public ArrayQuery(IJsonQueryable[] queries)
        {
            _queries = queries;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return new JsonArray(_queries.Select(query => query.Query(data)?.DeepClone()).ToArray());
        }

        public IEnumerable<IJsonQueryable> SubQueries => _queries;
    }

    public class QueryCollectionConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, IMultipleSubQuery
    {
        public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read(); // pass start array token
            reader.Read(); // pass keyword

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

    [JsonConverter(typeof(ObjectQueryConverter))]
    public class ObjectQuery : IJsonQueryable
    {
        internal const string Keyword = "object";

        public Dictionary<string, IJsonQueryable> PropertiesQueries { get; }

        public ObjectQuery(Dictionary<string, IJsonQueryable> propertiesQueries)
        {
            PropertiesQueries = propertiesQueries;
        }

        public JsonNode Query(JsonNode? data)
        {
            return new JsonObject(PropertiesQueries.Select(prop => KeyValuePair.Create(prop.Key, prop.Value.Query(data)?.DeepClone())));
        }
    }

    public class ObjectQueryConverter : JsonConverter<ObjectQuery>
    {
        public override ObjectQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            var objectQuery = new ObjectQuery(JsonSerializer.Deserialize<Dictionary<string, IJsonQueryable>>(ref reader)!);

            reader.Read();

            return objectQuery;
        }

        public override void Write(Utf8JsonWriter writer, ObjectQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            JsonSerializer.Serialize(writer, value.PropertiesQueries);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<FilterQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<FilterQuery>))]
    public class FilterQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "filter";

        private readonly IJsonQueryable _filter;

        public FilterQuery(IJsonQueryable filter)
        {
            _filter = filter;
        }

        public JsonNode? Query(JsonNode? data)
        {
            var result = new JsonArray();

            foreach (JsonNode? item in data!.AsArray())
            {
                JsonNode? predictResult = _filter.Query(item);

                if (predictResult is not null && predictResult.GetBooleanValue())
                {
                    result.Add(item?.DeepClone());
                }
            }

            return result;
        }

        public IJsonQueryable SubQuery => _filter;
    }

    internal class SingleQueryParameterParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable
    {
        public override TQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read(); // ')'

            return (TQuery)Activator.CreateInstance(typeof(TQuery), query);
        }
    }

    public class SingleQueryParameterConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, ISingleSubQuery
    {
        public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

            TQuery? queryable = (TQuery?)Activator.CreateInstance(typeof(TQuery), query);

            reader.Read();

            return queryable;
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubQuery);
            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(SortQueryConverter))]
    [JsonQueryConverter(typeof(SortQueryParserConverter))]
    public class SortQuery : IJsonQueryable
    {
        internal const string Keyword = "sort";

        public IJsonQueryable SubQuery { get; }
        public bool IsDesc { get; }

        public SortQuery(IJsonQueryable sortQuery, bool isDesc = false)
        {
            SubQuery = sortQuery;
            IsDesc = isDesc;
        }

        public JsonNode? Query(JsonNode? data)
        {
            if (data is null)
            {
                return null;
            }

            JsonArray array = data.AsArray();

            if (array.Count == 0)
            {
                return array;
            }

            JsonNode firstItem = SubQuery.Query(array[0])!;
            IEnumerable<JsonNode?> orderedNodes;
            if (firstItem.GetValueKind() == JsonValueKind.Number)
            {
                orderedNodes = IsDesc ? array.OrderByDescending(DecimalKeySelector) : array.OrderBy(DecimalKeySelector);
            }
            else // items kind is String
            {
                orderedNodes = IsDesc ? array.OrderByDescending(StringKeySelector, StringComparer.Ordinal) : array.OrderBy(StringKeySelector, StringComparer.Ordinal);
            }

            return new JsonArray(orderedNodes.Select(item => item?.DeepClone()).ToArray());

            decimal DecimalKeySelector(JsonNode? item)
            {
                return SubQuery.Query(item)!.GetValue<decimal>();
            }

            string StringKeySelector(JsonNode? item)
            {
                return SubQuery.Query(item)!.GetValue<string>();
            }
        }
    }

    public class SortQueryParserConverter : JsonQueryConverter<SortQuery>
    {
        public override SortQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable sortQuery;
            bool isDesc = false;

            if (reader.TokenType == JsonQueryTokenType.EndParenthesis)
            {
                sortQuery = new GetQuery(Array.Empty<object>());
            }
            else
            {
                sortQuery = JsonQueryParser.ParseQueryCombination(ref reader);
                
                reader.Read();

                if (reader.TokenType == JsonQueryTokenType.String && reader.GetString() == "desc")
                {
                    isDesc = true;
                    reader.Read();
                }
            }

            return new SortQuery(sortQuery, isDesc);
        }
    }

    public class SortQueryConverter : JsonConverter<SortQuery>
    {
        public override SortQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            bool isDesc = false;
            IJsonQueryable sortQuery;

            // try to go to first argument
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                sortQuery = new GetQuery(Array.Empty<object>());
            }
            else
            {
                sortQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

                reader.Read();

                // check to second argument
                if (reader.TokenType == JsonTokenType.String && reader.GetString() == "desc")
                {
                    isDesc = true;
                    reader.Read();
                }
            }

            return new SortQuery(sortQuery, isDesc);
        }

        public override void Write(Utf8JsonWriter writer, SortQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubQuery);

            if (value.IsDesc)
            {
                writer.WriteStringValue("desc");
            }

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<ReverseQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<ReverseQuery>))]
    public class ReverseQuery : IJsonQueryable
    {
        internal const string Keyword = "reverse";

        public JsonNode Query(JsonNode? data)
        {
            return new JsonArray(data!.AsArray().Reverse().Select(item => item?.DeepClone()).ToArray());
        }
    }

    public class ParameterlessQueryParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable, new()
    {
        public override TQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            return new TQuery();
        }
    }

    [JsonConverter(typeof(OperatorConverter<MultiplyOperator>))]
    public class MultiplyOperator : OperatorQuery
    {
        internal const string Keyword = "multiply";
        internal const string Operator = "*";

        public MultiplyOperator(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) * QueryRightDecimal(data);
        }
    }

    public class OperatorConverter<TOperator> : JsonConverter<TOperator> where TOperator : OperatorQuery
    {
        public override TOperator? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable left = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

            reader.Read();

            IJsonQueryable right = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

            reader.Read();

            return (TOperator?)Activator.CreateInstance(typeof(TOperator), left, right);
        }

        public override void Write(Utf8JsonWriter writer, TOperator value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.Left);
            JsonSerializer.Serialize(writer, value.Right);
            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(QueryCollectionConverter<PipeQuery>))]
    [JsonQueryConverter(typeof(QueryCollectionParserConverter<PipeQuery>))]
    public class PipeQuery : IJsonQueryable, IMultipleSubQuery
    {
        internal const string Keyword = "pipe";

        private readonly IJsonQueryable[] _queries;

        public PipeQuery(IJsonQueryable[] queries)
        {
            _queries = queries;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonNode? curNode = data;

            foreach (IJsonQueryable query in _queries)
            {
                curNode = query.Query(curNode);
            }

            return curNode;
        }

        public IEnumerable<IJsonQueryable> SubQueries => _queries;
    }

    public class QueryCollectionParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable
    {
        public override TQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            var queries = new List<IJsonQueryable>();
            while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
            {
                queries.Add(JsonQueryParser.ParseQueryCombination(ref reader));
                reader.Read();
            }

            return (TQuery)Activator.CreateInstance(typeof(TQuery), new object[] { queries.ToArray() });
        }
    }

    [JsonConverter(typeof(PickQueryConverter))]
    [JsonQueryConverter(typeof(PickQueryParserConverter))]
    public class PickQuery : IJsonQueryable
    {
        internal const string Keyword = "pick";

        public GetQuery[] GetQueries { get; }

        public PickQuery(GetQuery[] getQueries)
        {
            GetQueries = getQueries;
        }

        public JsonNode? Query(JsonNode? data)
        {
            if (data is JsonArray array)
            {
                var newArray = new JsonArray();

                foreach (JsonNode? item in array)
                {
                    newArray.Add(PickObject(item!.AsObject()));
                }

                return newArray;
            }

            if (data is JsonObject jsonObject)
            {
                return PickObject(jsonObject);
            }

            throw new InvalidOperationException("Invalid data format for pick query");
        }

        private JsonObject PickObject(JsonObject sourceObject)
        {
            IEnumerable<KeyValuePair<string, JsonNode?>> properties = GetQueries.Select(propQuery => propQuery.QueryPropertyNameAndValue(sourceObject))
                .Where(prop => prop.propertyName is not null)
                .Select(prop => KeyValuePair.Create(prop.propertyName!, prop.propertyValue?.DeepClone()));

            return new JsonObject(properties);
        }
    }

    public class PickQueryParserConverter : JsonQueryConverter<PickQuery>
    {
        public override PickQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            var getQueries = new List<GetQuery>();
            while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
            {
                IJsonQueryable query = JsonQueryParser.ParseSingleQuery(ref reader);
                if (query is not GetQuery getQuery)
                {
                    throw new JsonQueryParseException($"Invalid query type: {query.GetType()} for {typeof(PickQuery)}", reader.Position);
                }

                getQueries.Add(getQuery);

                reader.Read();
            }

            return new PickQuery(getQueries.ToArray());
        }
    }

    public class PickQueryConverter : JsonConverter<PickQuery>
    {
        public override PickQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            var getQueries = new List<GetQuery>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                getQueries.Add(JsonSerializer.Deserialize<GetQuery>(ref reader)!);

                reader.Read();
            }

            return new PickQuery(getQueries.ToArray());
        }

        public override void Write(Utf8JsonWriter writer, PickQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (GetQuery getQuery in value.GetQueries)
            {
                JsonSerializer.Serialize(writer, getQuery);
            }

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(MapObjectQueryConverter))]
    [JsonQueryConverter(typeof(MapObjectQueryParserConverter))]
    class MapObjectQuery : IJsonQueryable
    {
        internal const string Keyword = "mapObject";

        public IJsonQueryable KeyQuery { get; }
        public IJsonQueryable ValueQuery { get; }

        public MapObjectQuery(IJsonQueryable keyQuery, IJsonQueryable valueQuery)
        {
            KeyQuery = keyQuery;
            ValueQuery = valueQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            var result = new JsonObject();

            foreach (KeyValuePair<string, JsonNode?> prop in data!.AsObject())
            {
                var origPropObject = new JsonObject { { "key", prop.Key }, { "value", prop.Value?.DeepClone() } };
                result.Add(KeyQuery.Query(origPropObject)!.GetValue<string>(), ValueQuery.Query(origPropObject)?.DeepClone());
            }

            return result;
        }
    }

    internal class MapObjectQueryParserConverter : JsonQueryConverter<MapObjectQuery>
    {
        public override MapObjectQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            if (reader.TokenType != JsonQueryTokenType.StartBrace)
            {
                throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(MapObjectQuery)}", reader.Position);
            }

            ObjectQuery objectQuery = JsonQueryParser.ParseObjectQuery(ref reader);

            reader.Read();

            return new MapObjectQuery(objectQuery.PropertiesQueries["key"], objectQuery.PropertiesQueries["value"]);
        }
    }

    internal class MapObjectQueryConverter : JsonConverter<MapObjectQuery>
    {
        public override MapObjectQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            ObjectQuery objectQuery = JsonSerializer.Deserialize<ObjectQuery>(ref reader)!;

            reader.Read();

            return new MapObjectQuery(objectQuery.PropertiesQueries["key"], objectQuery.PropertiesQueries["value"]);
        }

        public override void Write(Utf8JsonWriter writer, MapObjectQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            var dic = new Dictionary<string, IJsonQueryable>(2) { { "key", value.KeyQuery }, { "value", value.ValueQuery } };

            JsonSerializer.Serialize(writer, dic);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<MapKeysQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapKeysQuery>))]
    class MapKeysQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "mapKeys";

        private readonly IJsonQueryable _keyQuery;

        public MapKeysQuery(IJsonQueryable keyQuery)
        {
            _keyQuery = keyQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            var result = new JsonObject();

            foreach (KeyValuePair<string, JsonNode?> prop in data!.AsObject())
            {
                result.Add(_keyQuery.Query(JsonValue.Create(prop.Key))!.GetValue<string>(), prop.Value?.DeepClone());
            }

            return result;
        }

        public IJsonQueryable SubQuery => _keyQuery;
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<MapValuesQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapValuesQuery>))]
    class MapValuesQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "mapValues";

        private readonly IJsonQueryable _valueQuery;

        public MapValuesQuery(IJsonQueryable valueQuery)
        {
            _valueQuery = valueQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            var result = new JsonObject();

            foreach (KeyValuePair<string, JsonNode?> prop in data!.AsObject())
            {
                result.Add(prop.Key, _valueQuery.Query(prop.Value)?.DeepClone());
            }

            return result;
        }

        public IJsonQueryable SubQuery => _valueQuery;
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<MapQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapQuery>))]
    class MapQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "map";

        private readonly IJsonQueryable _itemQuery;

        public MapQuery(IJsonQueryable itemQuery)
        {
            _itemQuery = itemQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return new JsonArray(data!.AsArray().Select(item => _itemQuery.Query(item)?.DeepClone()).ToArray());
        }

        public IJsonQueryable SubQuery => _itemQuery;
    }

    [JsonConverter(typeof(GetQueryParameterConverter<GroupByQuery>))]
    [JsonQueryConverter(typeof(GetQueryParameterParserConverter<GroupByQuery>))]
    class GroupByQuery : IJsonQueryable, ISubGetQuery
    {
        internal const string Keyword = "groupBy";

        public GroupByQuery(GetQuery getQuery)
        {
            SubGetQuery = getQuery;
        }

        public GetQuery SubGetQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            JsonArray array = data!.AsArray();

            IEnumerable<IGrouping<string, JsonNode?>> groupByResult = array.GroupBy(itemNode => SubGetQuery.Query(itemNode)!.GetValue<string>());
            IEnumerable<KeyValuePair<string, JsonNode?>> newProperties = groupByResult.Select(group => KeyValuePair.Create<string, JsonNode?>(group.Key, new JsonArray(group.Select(item => item?.DeepClone()).ToArray())));

            return new JsonObject(newProperties);
        }
    }

    internal class GetQueryParameterParserConverter<TQuery> : JsonQueryConverter<TQuery> where TQuery : IJsonQueryable
    {
        public override TQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            GetQuery getQuery = (GetQuery)JsonQueryParser.ParseSingleQuery(ref reader);

            reader.Read();

            return (TQuery)Activator.CreateInstance(typeof(TQuery), getQuery);
        }
    }

    [JsonConverter(typeof(GetQueryParameterConverter<KeyByQuery>))]
    [JsonQueryConverter(typeof(GetQueryParameterParserConverter<KeyByQuery>))]
    class KeyByQuery : IJsonQueryable, ISubGetQuery
    {
        internal const string Keyword = "keyBy";

        public KeyByQuery(GetQuery getQuery)
        {
            SubGetQuery = getQuery;
        }

        public GetQuery SubGetQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            var newProperties = new Dictionary<string, JsonNode?>();

            foreach (JsonNode? item in data!.AsArray())
            {
                JsonNode? keyNode = SubGetQuery.Query(item);

                string key = keyNode is null ? "null" : keyNode.ToString();

                newProperties.TryAdd(key, item?.DeepClone());
            }

            return new JsonObject(newProperties);
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<KeysQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<KeysQuery>))]
    class KeysQuery : IJsonQueryable
    {
        internal const string Keyword = "keys";

        public JsonNode? Query(JsonNode? data)
        {
            IEnumerable<JsonValue> keys = data!.AsObject().Select(prop => JsonValue.Create(prop.Key)!);

            return new JsonArray(keys.ToArray<JsonNode>());
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<ValuesQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<ValuesQuery>))]
    class ValuesQuery : IJsonQueryable
    {
        internal const string Keyword = "values";

        public JsonNode? Query(JsonNode? data)
        {
            IEnumerable<JsonNode?> values = data!.AsObject().Select(prop => prop.Value?.DeepClone());

            return new JsonArray(values.ToArray());
        }
    }

    internal class ParameterlessQueryConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, new()
    {
        public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            return new TQuery();
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.GetKeyword());
            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<FlattenQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<FlattenQuery>))]
    class FlattenQuery : IJsonQueryable
    {
        internal const string Keyword = "flatten";

        public JsonNode? Query(JsonNode? data)
        {
            IEnumerable<JsonNode?> flattenArray = data!.AsArray().SelectMany(item => item!.AsArray().Select(subItem => subItem?.DeepClone()));

            return new JsonArray(flattenArray.ToArray());
        }
    }

    [JsonConverter(typeof(JoinQueryConverter))]
    [JsonQueryConverter(typeof(JoinQueryParserConverter))]
    class JoinQuery : IJsonQueryable
    {
        internal const string Keyword = "join";

        public string Separator { get; }

        public JoinQuery(string separator = "")
        {
            Separator = separator;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return string.Join(Separator, data!.AsArray().Select(node => node!.GetValue<string>()));
        }
    }

    internal class JoinQueryParserConverter : JsonQueryConverter<JoinQuery>
    {
        public override JoinQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            if (reader.TokenType == JsonQueryTokenType.String)
            {
                var joinQuery = new JoinQuery(reader.GetString());
                reader.Read();

                return joinQuery;
            }

            return new JoinQuery();
        }
    }

    internal class JoinQueryConverter : JsonConverter<JoinQuery>
    {
        public override JoinQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            if (reader.TokenType == JsonTokenType.String)
            {
                var joinQuery = new JoinQuery(reader.GetString()!);
                reader.Read();

                return joinQuery;
            }

            return new JoinQuery();
        }

        public override void Write(Utf8JsonWriter writer, JoinQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.Separator);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(SplitQueryConverter))]
    [JsonQueryConverter(typeof(SplitQueryParserConverter))]
    class SplitQuery : IJsonQueryable
    {
        internal const string Keyword = "split";

        public IJsonQueryable SubQuery { get; }
        public string? Separator { get; }

        public SplitQuery(IJsonQueryable query, string? separator = null)
        {
            SubQuery = query;
            Separator = separator;
        }

        public JsonNode? Query(JsonNode? data)
        {
            string stringContent = SubQuery.Query(data)!.GetValue<string>();

            IEnumerable<string> words;
            if (Separator is null)
            {
                words = stringContent.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            }
            else if (Separator == string.Empty)
            {
                words = stringContent.Select(c => c.ToString());
            }
            else
            {
                words = stringContent.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            }

            return new JsonArray(words.Select(word => JsonValue.Create(word)).ToArray<JsonNode?>());
        }
    }

    internal class SplitQueryParserConverter : JsonQueryConverter<SplitQuery>
    {
        public override SplitQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            if (reader.TokenType == JsonQueryTokenType.String)
            {
                var splitQuery = new SplitQuery(query, reader.GetString());
                reader.Read();

                return splitQuery;
            }

            return new SplitQuery(query);
        }
    }

    internal class SplitQueryConverter : JsonConverter<SplitQuery>
    {
        public override SplitQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

            reader.Read();

            if (reader.TokenType == JsonTokenType.String)
            {
                var splitQuery = new SplitQuery(query, reader.GetString());
                reader.Read();

                return splitQuery;
            }

            return new SplitQuery(query);
        }

        public override void Write(Utf8JsonWriter writer, SplitQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubQuery);
            if (value.Separator is not null)
            {
                writer.WriteStringValue(value.Separator);
            }

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(SubstringQueryConverter))]
    [JsonQueryConverter(typeof(SubstringQueryParserConverter))]
    class SubstringQuery : IJsonQueryable
    {
        internal const string Keyword = "substring";

        public IJsonQueryable SubQuery { get; }
        public int StartIdx { get; }
        public int? EndIdx { get; }

        public SubstringQuery(IJsonQueryable query, int startIdx, int? endIdx = null)
        {
            SubQuery = query;
            StartIdx = startIdx < 0 ? 0 : startIdx;
            EndIdx = endIdx;
        }

        public JsonNode? Query(JsonNode? data)
        {
            string stringContent = SubQuery.Query(data)!.GetValue<string>();

            if (StartIdx >= stringContent.Length)
            {
                return string.Empty;
            }

            if (!EndIdx.HasValue || EndIdx.Value >= stringContent.Length)
            {
                return stringContent.Substring(StartIdx);
            }

            Debug.Assert(EndIdx.HasValue);

            if (StartIdx >= EndIdx.Value)
            {
                return string.Empty;
            }

            return stringContent.Substring(StartIdx, EndIdx.Value - StartIdx);
        }
    }

    internal class SubstringQueryParserConverter : JsonQueryConverter<SubstringQuery>
    {
        public override SubstringQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            if (reader.TokenType != JsonQueryTokenType.Number)
            {
                throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(SubstringQuery)}", reader.Position);
            }

            int start = (int)reader.GetDecimal();

            reader.Read();

            int? end;

            if (reader.TokenType == JsonQueryTokenType.Number)
            {
                end = (int)reader.GetDecimal();
                reader.Read();
            }
            else
            {
                end = null;
            }

            return new SubstringQuery(query, start, end);
        }
    }

    internal class SubstringQueryConverter : JsonConverter<SubstringQuery>
    {
        public override SubstringQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;
            
            reader.Read();

            int start = reader.GetInt32();
            
            reader.Read();

            int? end;

            if (reader.TokenType == JsonTokenType.Number)
            {
                end = reader.GetInt32();
                reader.Read();
            }
            else
            {
                end = null;
            }

            return new SubstringQuery(query, start, end);
        }

        public override void Write(Utf8JsonWriter writer, SubstringQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubQuery);
            writer.WriteNumberValue(value.StartIdx);
            if (value.EndIdx.HasValue)
            {
                writer.WriteNumberValue(value.EndIdx.Value);
            }

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<UniqQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<UniqQuery>))]
    class UniqQuery : IJsonQueryable
    {
        internal const string Keyword = "uniq";

        public JsonNode? Query(JsonNode? data)
        {
            var result = new List<JsonNode?>();

            foreach (JsonNode? item in data!.AsArray())
            {
                if (result.All(node => !JsonNode.DeepEquals(node, item)))
                {
                    result.Add(item?.DeepClone());
                }
            }

            return new JsonArray(result.ToArray());
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<UniqByQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<UniqByQuery>))]
    class UniqByQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "uniqBy";

        public UniqByQuery(IJsonQueryable query)
        {
            SubQuery = query;
        }

        public IJsonQueryable SubQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            var sourceArray = data!.AsArray();

            var resultArray = new List<(JsonNode? key, JsonNode? value)>(sourceArray.Count);

            foreach (JsonNode? item in sourceArray)
            {
                JsonNode? key = SubQuery.Query(item);

                if (resultArray.All(kv => !JsonNode.DeepEquals(kv.key, key)))
                {
                    resultArray.Add((key, item));
                }
            }

            return new JsonArray(resultArray.Select(kv => kv.value?.DeepClone()).ToArray());
        }
    }

    [JsonConverter(typeof(LimitQueryConverter))]
    [JsonQueryConverter(typeof(LimitQueryParserConverter))]
    class LimitQuery : IJsonQueryable
    {
        internal const string Keyword = "limit";

        public int LimitSize { get; }

        public LimitQuery(int limitSize)
        {
            LimitSize = limitSize;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonArray array = data!.AsArray();
            return new JsonArray(array.SkipLast(array.Count - LimitSize).Select(item => item?.DeepClone()).ToArray());
        }
    }

    internal class LimitQueryParserConverter : JsonQueryConverter<LimitQuery>
    {
        public override LimitQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            if (reader.TokenType != JsonQueryTokenType.Number)
            {
                throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(LimitQuery)}", reader.Position);
            }

            int limitSize = (int)reader.GetDecimal();

            reader.Read();

            return new LimitQuery(limitSize);
        }
    }

    internal class LimitQueryConverter : JsonConverter<LimitQuery>
    {
        public override LimitQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            int limitSize = reader.GetInt32();
            
            reader.Read();

            return new LimitQuery(limitSize);
        }

        public override void Write(Utf8JsonWriter writer, LimitQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteNumberValue(value.LimitSize);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<SizeQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<SizeQuery>))]
    class SizeQuery : IJsonQueryable
    {
        internal const string Keyword = "size";

        public JsonNode? Query(JsonNode? data)
        {
            if (data is JsonArray array)
            {
                return JsonValue.Create(array.Count);
            }

            return JsonValue.Create(data!.GetValue<string>().Length);
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<SumQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<SumQuery>))]
    class SumQuery : IJsonQueryable
    {
        internal const string Keyword = "sum";

        public JsonNode? Query(JsonNode? data)
        {
            return data!.AsArray().Sum(item => item!.GetValue<decimal>());
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<MinQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<MinQuery>))]
    class MinQuery : IJsonQueryable
    {
        internal const string Keyword = "min";

        public JsonNode? Query(JsonNode? data)
        {
            return JsonValue.Create(data!.AsArray().Min(item => item!.GetValue<decimal>()));
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<MaxQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<MaxQuery>))]
    class MaxQuery : IJsonQueryable
    {
        internal const string Keyword = "max";

        public JsonNode? Query(JsonNode? data)
        {
            return JsonValue.Create(data!.AsArray().Max(item => item!.GetValue<decimal>()));
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<ProdQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<ProdQuery>))]
    class ProdQuery : IJsonQueryable
    {
        internal const string Keyword = "prod";

        public JsonNode? Query(JsonNode? data)
        {
            decimal result = data!.AsArray().Select(item => item!.GetValue<decimal>()).Aggregate((pre, current) => current * pre);
            return JsonValue.Create(result);
        }
    }

    [JsonConverter(typeof(ParameterlessQueryConverter<AverageQuery>))]
    [JsonQueryConverter(typeof(ParameterlessQueryParserConverter<AverageQuery>))]
    class AverageQuery : IJsonQueryable
    {
        internal const string Keyword = "average";

        public JsonNode? Query(JsonNode? data)
        {
            decimal result = data!.AsArray().Average(item => item!.GetValue<decimal>());
            return JsonValue.Create(result);
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<NotQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<NotQuery>))]
    class NotQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "not";

        public NotQuery(IJsonQueryable query)
        {
            SubQuery = query;
        }

        public IJsonQueryable SubQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            bool queryResult = SubQuery.Query(data)!.GetBooleanValue();

            return JsonValue.Create(!queryResult);
        }
    }

    [JsonConverter(typeof(GetQueryParameterConverter<ExistsQuery>))]
    [JsonQueryConverter(typeof(GetQueryParameterParserConverter<ExistsQuery>))]
    class ExistsQuery : IJsonQueryable, ISubGetQuery
    {
        internal const string Keyword = "exists";

        public ExistsQuery(GetQuery getQuery)
        {
            SubGetQuery = getQuery;
        }

        public GetQuery SubGetQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            bool exist = SubGetQuery.QueryPropertyNameAndValue(data).exist;

            return JsonValue.Create(exist);
        }
    }

    [JsonConverter(typeof(IfQueryConverter))]
    [JsonQueryConverter(typeof(IfQueryParserConverter))]
    class IfQuery : IJsonQueryable
    {
        internal const string Keyword = "if";

        public IJsonQueryable IfSubQuery { get; }
        public IJsonQueryable ThenSubQuery { get; }
        public IJsonQueryable ElseSubQuery { get; }

        public IfQuery(IJsonQueryable ifQuery, IJsonQueryable thenQuery, IJsonQueryable elseQuery)
        {
            IfSubQuery = ifQuery;
            ThenSubQuery = thenQuery;
            ElseSubQuery = elseQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            bool condition = IfSubQuery.Query(data)!.GetBooleanValue();

            return condition ? ThenSubQuery.Query(data) : ElseSubQuery.Query(data);
        }
    }

    internal class IfQueryParserConverter : JsonQueryConverter<IfQuery>
    {
        public override IfQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable ifQuery = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            IJsonQueryable thenQuery = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            IJsonQueryable elseQuery = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            return new IfQuery(ifQuery, thenQuery, elseQuery);
        }
    }

    internal class IfQueryConverter : JsonConverter<IfQuery>
    {
        public override IfQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable ifQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;
            
            reader.Read();
            
            IJsonQueryable thenQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;
            
            reader.Read();
            
            IJsonQueryable elseQuery = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

            reader.Read();

            return new IfQuery(ifQuery, thenQuery, elseQuery);
        }

        public override void Write(Utf8JsonWriter writer, IfQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            JsonSerializer.Serialize(writer, value.IfSubQuery);
            JsonSerializer.Serialize(writer, value.ThenSubQuery);
            JsonSerializer.Serialize(writer, value.ElseSubQuery);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(RegexQueryConverter))]
    [JsonQueryConverter(typeof(RegexQueryParserConverter))]
    class RegexQuery : IJsonQueryable
    {
        internal const string Keyword = "regex";

        public IJsonQueryable SubQuery { get; }
        public string RegexValue { get; }
        public RegexOptions Options { get; }

        public RegexQuery(IJsonQueryable query, string regex, RegexOptions options)
        {
            SubQuery = query;
            RegexValue = regex;
            Options = options;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonNode? jsonNode = SubQuery.Query(data);

            string content = jsonNode!.GetValue<string>();

            return JsonValue.Create(Regex.IsMatch(content, RegexValue, Options));
        }
    }

    internal class RegexQueryParserConverter : JsonQueryConverter<RegexQuery>
    {
        public override RegexQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            if (reader.TokenType != JsonQueryTokenType.String)
            {
                throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(RegexQuery)}", reader.Position);
            }

            string regex = reader.GetString();

            reader.Read();

            RegexOptions option = RegexOptions.None;
            if (reader.TokenType == JsonQueryTokenType.String)
            {
                if (reader.GetString() == "i")
                {
                    option = RegexOptions.IgnoreCase;
                }

                reader.Read();
            }

            return new RegexQuery(query, regex, option);
        }
    }

    internal class RegexQueryConverter : JsonConverter<RegexQuery>
    {
        public override RegexQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;
            
            reader.Read();
            
            string regex = reader.GetString()!;

            reader.Read();

            RegexOptions option = RegexOptions.None;
            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.GetString() == "i")
                {
                    option = RegexOptions.IgnoreCase;
                }

                reader.Read();
            }

            return new RegexQuery(query, regex, option);
        }

        public override void Write(Utf8JsonWriter writer, RegexQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubQuery);
            writer.WriteStringValue(value.RegexValue);

            if (value.Options == RegexOptions.IgnoreCase)
            {
                writer.WriteStringValue("i");
            }

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(RoundQueryConverter))]
    [JsonQueryConverter(typeof(RoundQueryParserConverter))]
    class RoundQuery : IJsonQueryable
    {
        internal const string Keyword = "round";

        public IJsonQueryable SubQuery { get; }
        public int Digits { get; }

        public RoundQuery(IJsonQueryable query, int digits = 0)
        {
            SubQuery = query;
            Digits = digits;
        }

        public JsonNode? Query(JsonNode? data)
        {
            decimal value = SubQuery.Query(data)!.GetValue<decimal>();
            decimal roundResult = Math.Round(value, Digits, MidpointRounding.AwayFromZero);

            return JsonValue.Create(roundResult);
        }
    }

    internal class RoundQueryParserConverter : JsonQueryConverter<RoundQuery>
    {
        public override RoundQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonQueryParser.ParseQueryCombination(ref reader);

            reader.Read();

            int digits = 0;
            if (reader.TokenType == JsonQueryTokenType.Number)
            {
                digits = (int)reader.GetDecimal();
                reader.Read();
            }

            return new RoundQuery(query, digits);
        }
    }

    internal class RoundQueryConverter : JsonConverter<RoundQuery>
    {
        public override RoundQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            IJsonQueryable query = JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!;

            reader.Read();

            int digits = 0;
            if (reader.TokenType == JsonTokenType.Number)
            {
                digits = reader.GetInt32();
                reader.Read();
            }

            return new RoundQuery(query, digits);
        }

        public override void Write(Utf8JsonWriter writer, RoundQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            writer.WriteStringValue(value.GetKeyword());
            JsonSerializer.Serialize(writer, value.SubQuery);
            writer.WriteNumberValue(value.Digits);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<AbsQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<AbsQuery>))]
    class AbsQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "abs";

        public AbsQuery(IJsonQueryable query)
        {
            SubQuery = query;
        }

        public IJsonQueryable SubQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            decimal value = SubQuery.Query(data)!.GetValue<decimal>();

            return JsonValue.Create(Math.Abs(value));
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<NumberQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<NumberQuery>))]
    class NumberQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "number";

        public NumberQuery(IJsonQueryable query)
        {
            SubQuery = query;
        }

        public IJsonQueryable SubQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            string numericString = SubQuery.Query(data)!.GetValue<string>();

            return decimal.TryParse(numericString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal numericValue) 
                ? numericValue 
                : null;
        }
    }

    public interface ISingleSubQuery
    {
        IJsonQueryable SubQuery { get; }
    }

    public interface IMultipleSubQuery
    {
        IEnumerable<IJsonQueryable> SubQueries { get; }
    }

    public interface ISubGetQuery
    {
        GetQuery SubGetQuery { get; }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<StringQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<StringQuery>))]
    class StringQuery : IJsonQueryable, ISingleSubQuery
    {
        internal const string Keyword = "string";

        public StringQuery(IJsonQueryable query)
        {
            SubQuery = query;
        }

        public IJsonQueryable SubQuery { get; }

        public JsonNode? Query(JsonNode? data)
        {
            JsonNode? node = SubQuery.Query(data);

            return node is null ? "null" : node.ToString();
        }
    }

    [JsonConverter(typeof(OperatorConverter<EqQuery>))]
    class EqQuery : OperatorQuery
    {
        internal const string Keyword = "eq";
        internal const string Operator = "==";

        public EqQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            JsonNode? left = Left.Query(data);
            JsonNode? right = Right.Query(data);

            return JsonNode.DeepEquals(left, right);
        }
    }

    public abstract class OperatorQuery : IJsonQueryable
    {
        public IJsonQueryable Left { get; }
        public IJsonQueryable Right { get; }

        protected OperatorQuery(IJsonQueryable left, IJsonQueryable right)
        {
            Left = left;
            Right = right;
        }

        protected decimal QueryLeftDecimal(JsonNode? data) => Left.Query(data)!.GetValue<decimal>();
        protected decimal QueryRightDecimal(JsonNode? data) => Right.Query(data)!.GetValue<decimal>();

        protected bool QueryLeftBoolean(JsonNode? data) => QueryBoolean(Left, data);
        protected bool QueryRightBoolean(JsonNode? data) => QueryBoolean(Right, data);

        private static bool QueryBoolean(IJsonQueryable query, JsonNode? data)
        {
            JsonNode? jsonNode = query.Query(data);
            
            return jsonNode!.GetBooleanValue();
        }

        public abstract JsonNode? Query(JsonNode? data);
    }

    [JsonConverter(typeof(OperatorConverter<GtQuery>))]
    class GtQuery : OperatorQuery
    {
        internal const string Keyword = "gt";
        internal const string Operator = ">";

        public GtQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) > QueryRightDecimal(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<GteQuery>))]
    class GteQuery : OperatorQuery
    {
        internal const string Keyword = "gte";
        internal const string Operator = ">=";

        public GteQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) >= QueryRightDecimal(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<LtQuery>))]
    class LtQuery : OperatorQuery
    {
        internal const string Keyword = "lt";
        internal const string Operator = "<";

        public LtQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) < QueryRightDecimal(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<LteQuery>))]
    class LteQuery : OperatorQuery
    {
        internal const string Keyword = "lte";
        internal const string Operator = "<=";

        public LteQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) <= QueryRightDecimal(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<NeQuery>))]
    class NeQuery : OperatorQuery
    {
        internal const string Keyword = "ne";
        internal const string Operator = "!=";

        public NeQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return !JsonNode.DeepEquals(Left.Query(data), Right.Query(data));
        }
    }

    [JsonConverter(typeof(OperatorConverter<AndQuery>))]
    class AndQuery : OperatorQuery
    {
        internal const string Keyword = "and";
        internal const string Operator = "and";

        public AndQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftBoolean(data) && QueryRightBoolean(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<OrQuery>))]
    class OrQuery : OperatorQuery
    {
        internal const string Keyword = "or";
        internal const string Operator = "or";

        public OrQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftBoolean(data) || QueryRightBoolean(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<InQuery>))]
    class InQuery : OperatorQuery
    {
        internal const string Keyword = "in";
        internal const string Operator = "in";

        public InQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            JsonArray array = Right.Query(data)!.AsArray();

            JsonNode value = Left.Query(data)!;

            return array.Any(item => JsonNode.DeepEquals(item, value));
        }
    }

    [JsonConverter(typeof(OperatorConverter<NotInQuery>))]
    class NotInQuery : OperatorQuery
    {
        internal const string Keyword = "not in";
        internal const string Operator = "not in";

        public NotInQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            JsonArray array = Right.Query(data)!.AsArray();

            JsonNode value = Left.Query(data)!;

            return !array.Any(item => JsonNode.DeepEquals(item, value));
        }
    }

    [JsonConverter(typeof(OperatorConverter<AddOperator>))]
    class AddOperator : OperatorQuery
    {
        internal const string Keyword = "add";
        internal const string Operator = "+";

        public AddOperator(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            JsonNode? leftNode = Left.Query(data);
            JsonNode? rightNode = Right.Query(data);

            if (leftNode is not JsonValue leftValue || rightNode is not JsonValue rightValue)
            {
                return null;
            }

            if (leftValue.TryGetValue(out decimal leftNumber) && rightValue.TryGetValue(out decimal rightNumber))
            {
                return leftNumber + rightNumber;
            }

            if (leftValue.GetValueKind() == JsonValueKind.String || rightValue.GetValueKind() == JsonValueKind.String)
            {
                return leftValue.ToString() + rightValue;
            }

            return null; // todo
        }
    }

    [JsonConverter(typeof(OperatorConverter<SubtractQuery>))]
    class SubtractQuery : OperatorQuery
    {
        internal const string Keyword = "subtract";
        internal const string Operator = "-";

        public SubtractQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) - QueryRightDecimal(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<DivideQuery>))]
    class DivideQuery : OperatorQuery
    {
        internal const string Keyword = "divide";
        internal const string Operator = "/";

        public DivideQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) / QueryRightDecimal(data);
        }
    }

    [JsonConverter(typeof(OperatorConverter<PowQuery>))]
    class PowQuery : OperatorQuery
    {
        internal const string Keyword = "pow";
        internal const string Operator = "^";

        public PowQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return Math.Pow((double)QueryLeftDecimal(data), (double)QueryRightDecimal(data));
        }
    }

    [JsonConverter(typeof(OperatorConverter<ModQuery>))]
    class ModQuery : OperatorQuery
    {
        internal const string Keyword = "mod";
        internal const string Operator = "%";

        public ModQuery(IJsonQueryable left, IJsonQueryable right) : base(left, right)
        {
        }

        public override JsonNode? Query(JsonNode? data)
        {
            return QueryLeftDecimal(data) % QueryRightDecimal(data);
        }
    }

    internal class GetQueryParameterConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable, ISubGetQuery
    {
        public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            GetQuery getQuery = JsonSerializer.Deserialize<GetQuery>(ref reader)!;
            
            reader.Read();

            return (TQuery?)Activator.CreateInstance(typeof(TQuery), getQuery);
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            JsonSerializer.Serialize(writer, value.SubGetQuery);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(GetQueryConverter))]
    [JsonQueryConverter(typeof(GetQueryParserConverter))]
    public class GetQuery : IJsonQueryable
    {
        internal const string Keyword = "get";

        public object[] Path { get; }

        public GetQuery(object[] path)
        {
            Path = path;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return QueryPropertyNameAndValue(data).propertyValue;
        }

        public (string? propertyName, JsonNode? propertyValue, bool exist) QueryPropertyNameAndValue(JsonNode? data)
        {
            JsonNode? curNode = data;
            foreach (object segment in Path)
            {
                if (curNode is null)
                {
                    return (GetTheLastPropertyName(), null, false);
                }

                if (curNode is JsonObject jsonObject)
                {
                    string propertyName = (string)segment;

                    if (!jsonObject.TryGetPropertyValue(propertyName, out curNode))
                    {
                        return (GetTheLastPropertyName(), null, false);
                    }
                }
                else
                {
                    JsonArray jsonArray = curNode.AsArray();

                    int index = (int)segment;

                    if (index < jsonArray.Count)
                    {
                        curNode = jsonArray[index];
                    }
                    else
                    {
                        return (GetTheLastPropertyName(), null, false);
                    }
                }
            }

            return (GetTheLastPropertyName(), curNode, true);
        }

        private string? GetTheLastPropertyName()
        {
            return Path.Length == 0 ? null : Path[Path.Length - 1].ToString();
        }
    }

    public class GetQueryParserConverter : JsonQueryConverter<GetQuery>
    {
        public override GetQuery Read(ref JsonQueryReader reader)
        {
            reader.Read();
            reader.Read();

            var propertyPath = new List<object>();
            while (reader.TokenType != JsonQueryTokenType.EndParenthesis)
            {
                object segment;
                if (reader.TokenType == JsonQueryTokenType.String)
                {
                    segment = reader.GetString();
                }
                else if (reader.TokenType == JsonQueryTokenType.Number)
                {
                    segment = (int)reader.GetDecimal();
                }
                else
                {
                    throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for {typeof(GetQuery)}", reader.Position);
                }

                propertyPath.Add(segment);

                reader.Read();
            }

            return new GetQuery(propertyPath.ToArray());
        }
    }

    public class GetQueryConverter : JsonConverter<GetQuery>
    {
        public override GetQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();

            reader.Read();

            var propertyPath = new List<object>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                object segment;
                if (reader.TokenType == JsonTokenType.String)
                {
                    segment = reader.GetString()!;
                }
                else
                {
                    segment = reader.GetInt32();
                }
                propertyPath.Add(segment);

                reader.Read();
            }

            return new GetQuery(propertyPath.ToArray());
        }

        public override void Write(Utf8JsonWriter writer, GetQuery value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (object item in value.Path)
            {
                if (item is string stringItem)
                {
                    writer.WriteStringValue(stringItem);
                }
                else
                {
                    Debug.Assert(item is int);

                    writer.WriteNumberValue((int)item);
                }
            }

            writer.WriteEndArray();
        }
    }

    public class ConstQueryable : IJsonQueryable
    {
        public JsonNode? Value { get; }

        public ConstQueryable(JsonNode? value)
        {
            Value = value;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return Value;
        }
    }

    [JsonConverter(typeof(JsonQueryableConverter))]
    public interface IJsonQueryable
    {
        JsonNode? Query(JsonNode? data);
    }

    public class JsonQueryableConverter : JsonConverter<IJsonQueryable>
    {
        public override IJsonQueryable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader detectorReader = reader;

            if (detectorReader.TokenType == JsonTokenType.StartObject)
            {
                throw new JsonException("Invalid json format for json query");
            }

            if (detectorReader.TokenType == JsonTokenType.StartArray)
            {
                detectorReader.Read();

                string queryKeyword = detectorReader.GetString()!;

                if (JsonQueryableRegistry.TryGetQueryableType(queryKeyword, out Type? queryableType))
                {
                    return (IJsonQueryable)JsonSerializer.Deserialize(ref reader, queryableType)!;
                }

                throw new JsonException($"Invalid json query keyword: {queryKeyword}");
            }

            return new ConstQueryable(JsonNode.Parse(ref reader));
        }

        public override void Write(Utf8JsonWriter writer, IJsonQueryable value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType());
        }

        public override bool HandleNull => true;
    }
}
