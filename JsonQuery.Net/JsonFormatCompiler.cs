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

    static class JsonQueryableRegistry
    {
        private static readonly Dictionary<string, Type> JsonQueryables;

        static JsonQueryableRegistry()
        {
            JsonQueryables = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && typeof(IJsonQueryable).IsAssignableFrom(type) && type != typeof(ConstQueryable)).ToDictionary(type => (string)type.GetField("Keyword", BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue());
        }

        public static bool TryGetQueryableType(string keyword, [NotNullWhen(true)]out Type? queryType) => JsonQueryables.TryGetValue(keyword, out queryType);
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
    public class ArrayQuery : IJsonQueryable
    {
        internal const string Keyword = "array";

        public IJsonQueryable[] Queries { get; }

        public ArrayQuery(IJsonQueryable[] queries)
        {
            Queries = queries;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return new JsonArray(Queries.Select(query => query.Query(data)?.DeepClone()).ToArray());
        }
    }

    public class QueryCollectionConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read(); // pass start array token
            reader.Read(); // pass keyword

            var queries = new List<IJsonQueryable>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                queries.Add(JsonSerializer.Deserialize<IJsonQueryable>(ref reader)!);
                
                reader.Read();
            }

            return (T)Activator.CreateInstance(typeof(T), new object[] { queries.ToArray() });
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<FilterQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<FilterQuery>))]
    public class FilterQuery : IJsonQueryable
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

    public class SingleQueryParameterConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(SortQueryConverter))]
    [JsonQueryConverter(typeof(SortQueryParserConverter))]
    public class SortQuery : IJsonQueryable
    {
        internal const string Keyword = "sort";

        private readonly IJsonQueryable _sortQuery;
        private readonly bool _isDesc;

        public SortQuery(IJsonQueryable sortQuery, bool isDesc = false)
        {
            _sortQuery = sortQuery;
            _isDesc = isDesc;
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

            JsonNode firstItem = _sortQuery.Query(array[0])!;
            IEnumerable<JsonNode?> orderedNodes;
            if (firstItem.GetValueKind() == JsonValueKind.Number)
            {
                orderedNodes = _isDesc ? array.OrderByDescending(DecimalKeySelector) : array.OrderBy(DecimalKeySelector);
            }
            else // items kind is String
            {
                orderedNodes = _isDesc ? array.OrderByDescending(StringKeySelector, StringComparer.Ordinal) : array.OrderBy(StringKeySelector, StringComparer.Ordinal);
            }

            return new JsonArray(orderedNodes.Select(item => item?.DeepClone()).ToArray());

            decimal DecimalKeySelector(JsonNode? item)
            {
                return _sortQuery.Query(item)!.GetValue<decimal>();
            }

            string StringKeySelector(JsonNode? item)
            {
                return _sortQuery.Query(item)!.GetValue<string>();
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
                }

                if (reader.TokenType != JsonTokenType.EndArray)
                {
                    reader.Read(); // to the end of current json node
                }
            }

            return new SortQuery(sortQuery, isDesc);
        }

        public override void Write(Utf8JsonWriter writer, SortQuery value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
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

    public class OperatorConverter<TOperator> : JsonConverter<TOperator>
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(QueryCollectionConverter<PipeQuery>))]
    [JsonQueryConverter(typeof(QueryCollectionParserConverter<PipeQuery>))]
    public class PipeQuery : IJsonQueryable
    {
        internal const string Keyword = "pipe";

        public IJsonQueryable[] Queries { get; }

        public PipeQuery(IJsonQueryable[] queries)
        {
            Queries = queries;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonNode? curNode = data;

            foreach (IJsonQueryable query in Queries)
            {
                curNode = query.Query(curNode);
            }

            return curNode;
        }
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

        private readonly GetQuery[] _getQueries;

        public PickQuery(GetQuery[] getQueries)
        {
            _getQueries = getQueries;
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
            IEnumerable<KeyValuePair<string, JsonNode?>> properties = _getQueries.Select(propQuery => propQuery.QueryPropertyNameAndValue(sourceObject))
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(MapObjectQueryConverter))]
    [JsonQueryConverter(typeof(MapObjectQueryParserConverter))]
    class MapObjectQuery : IJsonQueryable
    {
        internal const string Keyword = "mapObject";

        private readonly IJsonQueryable _keyQuery;
        private readonly IJsonQueryable _valueQuery;

        public MapObjectQuery(IJsonQueryable keyQuery, IJsonQueryable valueQuery)
        {
            _keyQuery = keyQuery;
            _valueQuery = valueQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            var result = new JsonObject();

            foreach (KeyValuePair<string, JsonNode?> prop in data!.AsObject())
            {
                var origPropObject = new JsonObject { { "key", prop.Key }, { "value", prop.Value?.DeepClone() } };
                result.Add(_keyQuery.Query(origPropObject)!.GetValue<string>(), _valueQuery.Query(origPropObject)?.DeepClone());
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<MapKeysQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapKeysQuery>))]
    class MapKeysQuery : IJsonQueryable
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
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<MapValuesQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapValuesQuery>))]
    class MapValuesQuery : IJsonQueryable
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
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<MapQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<MapQuery>))]
    class MapQuery : IJsonQueryable
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
    }

    [JsonConverter(typeof(GetQueryParameterConverter<GroupByQuery>))]
    [JsonQueryConverter(typeof(GetQueryParameterParserConverter<GroupByQuery>))]
    class GroupByQuery : IJsonQueryable
    {
        internal const string Keyword = "groupBy";

        private readonly GetQuery _getQuery;

        public GroupByQuery(GetQuery getQuery)
        {
            _getQuery = getQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonArray array = data!.AsArray();

            IEnumerable<IGrouping<string, JsonNode?>> groupByResult = array.GroupBy(itemNode => _getQuery.Query(itemNode)!.GetValue<string>());
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
    class KeyByQuery : IJsonQueryable
    {
        internal const string Keyword = "keyBy";

        private readonly GetQuery _getQuery;

        public KeyByQuery(GetQuery getQuery)
        {
            _getQuery = getQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            var newProperties = new Dictionary<string, JsonNode?>();

            foreach (JsonNode? item in data!.AsArray())
            {
                JsonNode? keyNode = _getQuery.Query(item);

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

    internal class ParameterlessQueryConverter<TQuery> : JsonConverter<TQuery> where TQuery : new()
    {
        public override TQuery? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            reader.Read();

            return new TQuery();
        }

        public override void Write(Utf8JsonWriter writer, TQuery value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
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

        private readonly string _separator;

        public JoinQuery(string separator = "")
        {
            _separator = separator;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return string.Join(_separator, data!.AsArray().Select(node => node!.GetValue<string>()));
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(SplitQueryConverter))]
    [JsonQueryConverter(typeof(SplitQueryParserConverter))]
    class SplitQuery : IJsonQueryable
    {
        internal const string Keyword = "split";

        private readonly IJsonQueryable _query;
        private readonly string? _separator;

        public SplitQuery(IJsonQueryable query, string? separator = null)
        {
            _query = query;
            _separator = separator;
        }

        public JsonNode? Query(JsonNode? data)
        {
            string stringContent = _query.Query(data)!.GetValue<string>();

            IEnumerable<string> words;
            if (_separator is null)
            {
                words = stringContent.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            }
            else if (_separator == string.Empty)
            {
                words = stringContent.Select(c => c.ToString());
            }
            else
            {
                words = stringContent.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(SubstringQueryConverter))]
    [JsonQueryConverter(typeof(SubstringQueryParserConverter))]
    class SubstringQuery : IJsonQueryable
    {
        internal const string Keyword = "substring";

        private readonly IJsonQueryable _query;
        private readonly int _startIdx;
        private readonly int? _endIdx;

        public SubstringQuery(IJsonQueryable query, int startIdx, int? endIdx = null)
        {
            _query = query;
            _startIdx = startIdx < 0 ? 0 : startIdx;
            _endIdx = endIdx;
        }

        public JsonNode? Query(JsonNode? data)
        {
            string stringContent = _query.Query(data)!.GetValue<string>();

            if (_startIdx >= stringContent.Length)
            {
                return string.Empty;
            }

            if (!_endIdx.HasValue || _endIdx.Value >= stringContent.Length)
            {
                return stringContent.Substring(_startIdx);
            }

            Debug.Assert(_endIdx.HasValue);

            if (_startIdx >= _endIdx.Value)
            {
                return string.Empty;
            }

            return stringContent.Substring(_startIdx, _endIdx.Value - _startIdx);
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
            throw new NotImplementedException();
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
    class UniqByQuery : IJsonQueryable
    {
        internal const string Keyword = "uniqBy";

        private readonly IJsonQueryable _query;

        public UniqByQuery(IJsonQueryable query)
        {
            _query = query;
        }

        public JsonNode? Query(JsonNode? data)
        {
            var sourceArray = data!.AsArray();

            var resultArray = new List<(JsonNode? key, JsonNode? value)>(sourceArray.Count);

            foreach (JsonNode? item in sourceArray)
            {
                JsonNode? key = _query.Query(item);

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

        private readonly int _limitSize;

        public LimitQuery(int limitSize)
        {
            _limitSize = limitSize;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonArray array = data!.AsArray();
            return new JsonArray(array.SkipLast(array.Count - _limitSize).Select(item => item?.DeepClone()).ToArray());
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
            throw new NotImplementedException();
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
    class NotQuery : IJsonQueryable
    {
        internal const string Keyword = "not";

        private readonly IJsonQueryable _query;

        public NotQuery(IJsonQueryable query)
        {
            _query = query;
        }

        public JsonNode? Query(JsonNode? data)
        {
            bool queryResult = _query.Query(data)!.GetBooleanValue();

            return JsonValue.Create(!queryResult);
        }
    }

    [JsonConverter(typeof(GetQueryParameterConverter<ExistsQuery>))]
    [JsonQueryConverter(typeof(GetQueryParameterParserConverter<ExistsQuery>))]
    class ExistsQuery : IJsonQueryable
    {
        internal const string Keyword = "exists";

        private readonly GetQuery _getQuery;

        public ExistsQuery(GetQuery getQuery)
        {
            _getQuery = getQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            bool exist = _getQuery.QueryPropertyNameAndValue(data).exist;

            return JsonValue.Create(exist);
        }
    }

    [JsonConverter(typeof(IfQueryConverter))]
    [JsonQueryConverter(typeof(IfQueryParserConverter))]
    class IfQuery : IJsonQueryable
    {
        internal const string Keyword = "if";

        private readonly IJsonQueryable _ifQuery;
        private readonly IJsonQueryable _thenQuery;
        private readonly IJsonQueryable _elseQuery;

        public IfQuery(IJsonQueryable ifQuery, IJsonQueryable thenQuery, IJsonQueryable elseQuery)
        {
            _ifQuery = ifQuery;
            _thenQuery = thenQuery;
            _elseQuery = elseQuery;
        }

        public JsonNode? Query(JsonNode? data)
        {
            bool condition = _ifQuery.Query(data)!.GetBooleanValue();

            return condition ? _thenQuery.Query(data) : _elseQuery.Query(data);
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(RegexQueryConverter))]
    [JsonQueryConverter(typeof(RegexQueryParserConverter))]
    class RegexQuery : IJsonQueryable
    {
        internal const string Keyword = "regex";

        private readonly IJsonQueryable _query;
        private readonly string _regex;
        private readonly RegexOptions _options;

        public RegexQuery(IJsonQueryable query, string regex, RegexOptions options)
        {
            _query = query;
            _regex = regex;
            _options = options;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonNode? jsonNode = _query.Query(data);

            string content = jsonNode!.GetValue<string>();

            return JsonValue.Create(Regex.IsMatch(content, _regex, _options));
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(RoundQueryConverter))]
    [JsonQueryConverter(typeof(RoundQueryParserConverter))]
    class RoundQuery : IJsonQueryable
    {
        internal const string Keyword = "round";

        private readonly IJsonQueryable _query;
        private readonly int _digits;

        public RoundQuery(IJsonQueryable query, int digits = 0)
        {
            _query = query;
            _digits = digits;
        }

        public JsonNode? Query(JsonNode? data)
        {
            decimal value = _query.Query(data)!.GetValue<decimal>();
            decimal roundResult = Math.Round(value, _digits, MidpointRounding.AwayFromZero);

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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<AbsQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<AbsQuery>))]
    class AbsQuery : IJsonQueryable
    {
        internal const string Keyword = "abs";

        private readonly IJsonQueryable _query;

        public AbsQuery(IJsonQueryable query)
        {
            _query = query;
        }

        public JsonNode? Query(JsonNode? data)
        {
            decimal value = _query.Query(data)!.GetValue<decimal>();

            return JsonValue.Create(Math.Abs(value));
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<NumberQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<NumberQuery>))]
    class NumberQuery : IJsonQueryable
    {
        internal const string Keyword = "number";

        private readonly IJsonQueryable _query;

        public NumberQuery(IJsonQueryable query)
        {
            _query = query;
        }

        public JsonNode? Query(JsonNode? data)
        {
            string numericString = _query.Query(data)!.GetValue<string>();

            return decimal.TryParse(numericString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal numericValue) 
                ? numericValue 
                : null;
        }
    }

    [JsonConverter(typeof(SingleQueryParameterConverter<StringQuery>))]
    [JsonQueryConverter(typeof(SingleQueryParameterParserConverter<StringQuery>))]
    class StringQuery : IJsonQueryable
    {
        internal const string Keyword = "string";

        private readonly IJsonQueryable _query;

        public StringQuery(IJsonQueryable query)
        {
            _query = query;
        }

        public JsonNode? Query(JsonNode? data)
        {
            JsonNode? node = _query.Query(data);

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
        protected readonly IJsonQueryable Left;
        protected readonly IJsonQueryable Right;

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

    internal class GetQueryParameterConverter<TQuery> : JsonConverter<TQuery> where TQuery : IJsonQueryable
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
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(GetQueryConverter))]
    [JsonQueryConverter(typeof(GetQueryParserConverter))]
    public class GetQuery : IJsonQueryable
    {
        internal const string Keyword = "get";

        private readonly object[] _path;

        public GetQuery(object[] path)
        {
            _path = path;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return QueryPropertyNameAndValue(data).propertyValue;
        }

        public (string? propertyName, JsonNode? propertyValue, bool exist) QueryPropertyNameAndValue(JsonNode? data)
        {
            JsonNode? curNode = data;
            foreach (object segment in _path)
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
            return _path.Length == 0 ? null : _path[_path.Length - 1].ToString();
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
            throw new NotImplementedException();
        }
    }

    public class ConstQueryable : IJsonQueryable
    {
        private readonly JsonNode? _value;

        public ConstQueryable(JsonNode? value)
        {
            _value = value;
        }

        public JsonNode? Query(JsonNode? data)
        {
            return _value;
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
            throw new NotImplementedException();
        }

        public override bool HandleNull => true;
    }
}
