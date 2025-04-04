using System.Diagnostics;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

public static class JsonQueryParser
{
    public static IJsonQueryable ParseQueryCombination(ref JsonQueryReader reader)
    {
        var pipedQueries = new List<IJsonQueryable>();

        while (true)
        {
            pipedQueries.Add(ParseOperatorQuery(ref reader));

            if (!TryMoveNextIfNextIsNotTerminal(ref reader, IsTerminalToken))
            {
                break;
            }

            Debug.Assert(reader.TokenType == JsonQueryTokenType.Pipe);

            reader.Read();
        }

        return pipedQueries.Count == 1 ? pipedQueries[0] : new PipeQuery(pipedQueries.ToArray());

        static bool IsTerminalToken(JsonQueryTokenType tokenType)
        {
            return tokenType != JsonQueryTokenType.Pipe;
        }
    }

    private static IJsonQueryable ParseOperatorQuery(ref JsonQueryReader reader)
    {
        var operatorExpressionTree = new OperatorExpressionTree();

        operatorExpressionTree.AppendOperandQuery(ParseSingleQuery(ref reader));

        while (TryMoveNextIfNextIsNotTerminal(ref reader, IsTerminalToken))
        {
            Debug.Assert(reader.TokenType == JsonQueryTokenType.Operator);

            operatorExpressionTree.AppendOperator(reader.GetOperator());

            reader.Read();

            operatorExpressionTree.AppendOperandQuery(ParseSingleQuery(ref reader));
        }

        return operatorExpressionTree.Evaluate();

        static bool IsTerminalToken(JsonQueryTokenType tokenType)
        {
            return tokenType != JsonQueryTokenType.Operator;
        }
    }

    private static bool TryMoveNextIfNextIsNotTerminal(ref JsonQueryReader reader, Func<JsonQueryTokenType, bool> isTerminalTokenFunc)
    {
        JsonQueryReader nextReader = reader.Next();
        if (nextReader.IsNull || isTerminalTokenFunc(nextReader.TokenType))
        {
            return false;
        }

        reader = nextReader;

        return true;
    }

    public static IJsonQueryable ParseSingleQuery(ref JsonQueryReader reader)
    {
        IJsonQueryable query;
        switch (reader.TokenType)
        {
            case JsonQueryTokenType.StartParenthesis:
                query = ParseParenthesisQuery(ref reader);

                Debug.Assert(reader.TokenType == JsonQueryTokenType.EndParenthesis);
                return query;

            case JsonQueryTokenType.FunctionName:
                query = ParseFunctionQuery(ref reader);

                Debug.Assert(reader.TokenType == JsonQueryTokenType.EndParenthesis);
                return query;

            case JsonQueryTokenType.PropertyPath:
                return ParsePropertyPath(ref reader);

            case JsonQueryTokenType.StartBrace:
                query = ParseObjectQuery(ref reader);

                Debug.Assert(reader.TokenType == JsonQueryTokenType.EndBrace);
                return query;
            case JsonQueryTokenType.StartBracket:

                query = ParseArrayQuery(ref reader);

                Debug.Assert(reader.TokenType == JsonQueryTokenType.EndBracket);
                return query;

            case JsonQueryTokenType.String:
                return new ConstQueryable(reader.GetString());

            case JsonQueryTokenType.Number:
                return new ConstQueryable(reader.GetDecimal());

            case JsonQueryTokenType.True:
                return new ConstQueryable(true);

            case JsonQueryTokenType.False:
                return new ConstQueryable(false);

            case JsonQueryTokenType.Null:
                return new ConstQueryable(null);
            
            default:
                throw new InvalidOperationException($"invalid token type for single query: {reader.TokenType}");
        }
    }

    internal static ObjectQuery ParseObjectQuery(ref JsonQueryReader reader)
    {
        reader.Read();

        var propertiesQueries = new Dictionary<string, IJsonQueryable>();

        while (reader.TokenType != JsonQueryTokenType.EndBrace)
        {
            string propertyName = reader.GetPropertyName();
            reader.Read();
            IJsonQueryable query = ParseQueryCombination(ref reader);

            propertiesQueries[propertyName] = query;
            reader.Read();
        }

        return new ObjectQuery(propertiesQueries);
    }

    private static IJsonQueryable ParseArrayQuery(ref JsonQueryReader reader)
    {
        reader.Read();

        var queries = new List<IJsonQueryable>();

        while (reader.TokenType != JsonQueryTokenType.EndBracket)
        {
            queries.Add(ParseQueryCombination(ref reader));

            reader.Read();

        }

        return new ArrayQuery(queries.ToArray());
    }

    private static IJsonQueryable ParsePropertyPath(ref JsonQueryReader reader)
    {
        return new GetQuery(reader.GetPropertyPath());
    }

    private static IJsonQueryable ParseFunctionQuery(ref JsonQueryReader reader)
    {
        string functionName = reader.GetFunctionName();

        bool canFindFunction = JsonQueryableRegistry.TryGetQueryableType(functionName, out Type? queryableType);
        Debug.Assert(canFindFunction);
        Debug.Assert(queryableType is not null);

        return FunctionQuerySerializer.Deserialize(ref reader, queryableType);
    }

    private static IJsonQueryable ParseParenthesisQuery(ref JsonQueryReader reader)
    {
        reader.Read();

        IJsonQueryable query = ParseQueryCombination(ref reader);

        reader.Read();

        Debug.Assert(reader.TokenType == JsonQueryTokenType.EndParenthesis);

        return query;
    }

    // TODO: support more types when need
    internal static object Deserialize(ref JsonQueryReader reader, Type returnType)
    {
        if (returnType == typeof(int))
        {
            if (reader.TokenType != JsonQueryTokenType.Number)
            {
                throw new JsonQueryParseException($"Invalid token type: {reader.TokenType} for 'int32'", reader.Position);
            }

            return (int)reader.GetDecimal();
        }

        if (typeof(IJsonQueryable).IsAssignableFrom(returnType))
        {
            IJsonQueryable query = ParseQueryCombination(ref reader);

            if (returnType.IsInstanceOfType(query))
            {
                return query;
            }

            if (query is ConstQueryable)
            {
                throw new JsonQueryParseException($"Invalid const query for '{returnType}'", reader.Position);
            }

            throw new JsonQueryParseException($"Invalid query: {query.GetKeyword()} for '{returnType}'", reader.Position);
        }

        throw new NotSupportedException($"Not support to deserialize '{returnType}' for json query currently");
    }
}

internal readonly ref struct OperatorExpressionTree
{
    private readonly LinkedList<string> _operatorsList;
    private readonly LinkedList<IJsonQueryable> _operandQueriesList;

    public OperatorExpressionTree()
    {
        _operatorsList = new();
        _operandQueriesList = new();
    }

    public void AppendOperator(string operatorKeyword)
    {
        _operatorsList.AddLast(operatorKeyword);
    }

    public void AppendOperandQuery(IJsonQueryable query)
    {
        _operandQueriesList.AddLast(query);
    }

    public IJsonQueryable Evaluate()
    {
        if (_operatorsList.Count > 0)
        {
            string firstOperator = _operatorsList.First.Value;

            if (OperatorRegistry.IsVarargOperator(firstOperator) && _operatorsList.All(operatorItem => operatorItem == firstOperator))
            {
                return CreateVarArgOperatorQuery(firstOperator, _operandQueriesList);
            }
        }

        while (_operatorsList.Count != 0)
        {
            LinkedListNode<string>? highestPrecedenceOperatorNode, curOperatorNode;
            highestPrecedenceOperatorNode = curOperatorNode = _operatorsList.First;
            LinkedListNode<IJsonQueryable> leftOperandOfHighestPrecedenceOperator, curLeftOperandNode;
            leftOperandOfHighestPrecedenceOperator = curLeftOperandNode = _operandQueriesList.First;

            while (curOperatorNode is not null)
            {
                if (OperatorRegistry.FindOperatorPrecedenceValue(curOperatorNode.Value) > OperatorRegistry.FindOperatorPrecedenceValue(highestPrecedenceOperatorNode.Value))
                {
                    highestPrecedenceOperatorNode = curOperatorNode;
                    leftOperandOfHighestPrecedenceOperator = curLeftOperandNode;
                }

                curOperatorNode = curOperatorNode.Next;
                curLeftOperandNode = curLeftOperandNode.Next!;
            }

            string operatorName = highestPrecedenceOperatorNode.Value;
            IJsonQueryable leftOperandQuery = leftOperandOfHighestPrecedenceOperator.Value;
            IJsonQueryable rightOperandQuery = leftOperandOfHighestPrecedenceOperator.Next!.Value;

            IJsonQueryable newOperatorQuery = CreateTwoArgsOperatorQuery(operatorName, leftOperandQuery, rightOperandQuery);

            _operatorsList.Remove(highestPrecedenceOperatorNode);
            _operandQueriesList.Remove(leftOperandOfHighestPrecedenceOperator.Next);
            leftOperandOfHighestPrecedenceOperator.Value = newOperatorQuery;
        }

        Debug.Assert(_operandQueriesList.Count == 1);
        return _operandQueriesList.First.Value;
    }

    private static IJsonQueryable CreateTwoArgsOperatorQuery(string operatorName, IJsonQueryable leftQuery, IJsonQueryable rightQuery)
    {
        Type operatorType = OperatorRegistry.FindOperatorType(operatorName);

        return (IJsonQueryable)Activator.CreateInstance(operatorType, leftQuery, rightQuery);
    }

    private static IJsonQueryable CreateVarArgOperatorQuery(string operatorName, IEnumerable<IJsonQueryable> operandQueries)
    {
        Type operatorType = OperatorRegistry.FindOperatorType(operatorName);

        return (IJsonQueryable)Activator.CreateInstance(operatorType, new object[] { operandQueries.ToArray() });
    }
}