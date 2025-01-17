using System.Diagnostics;

namespace JsonQuery.Net;

public static class JsonQueryParser
{
    public static IJsonQueryable Parse(string jsonQuery)
    {
        JsonQueryReader reader = new JsonQueryReader(jsonQuery);
        
        reader.Read();
        IJsonQueryable jsonQueryable = ParseQueryCombination(ref reader);

        return jsonQueryable;
    }

    internal static IJsonQueryable ParseQueryCombination(ref JsonQueryReader reader)
    {
        var pipedQueries = new List<IJsonQueryable>();

        while (true)
        {
            pipedQueries.Add(ParseOperatorQuery(ref reader));

            if (!TryMoveNextIfNextIsNotTerminal(ref reader, IsTerminalToken))
            {
                break;
            }

            // here should be reader.TokenType == JsonQueryTokenType.Pipe
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
        IJsonQueryable query = ParseSingleQuery(ref reader);

        if (!TryMoveNextIfNextIsNotTerminal(ref reader, IsTerminalToken))
        {
            return query;
        }

        // here should be JsonQueryTokenType.Operator
        string operatorName = reader.GetOperator();

        reader.Read();

        IJsonQueryable rightQuery = ParseSingleQuery(ref reader);

        return CreateOperatorQuery(operatorName, query, rightQuery);

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

    internal static IJsonQueryable ParseSingleQuery(ref JsonQueryReader reader)
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
                throw new InvalidOperationException("invalid token type for single query");
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

        Type queryableType = JsonQueryableRegistry.FindQueryableType(functionName);

        return FunctionQuerySerializer.Deserialize(ref reader, queryableType);
    }

    private static IJsonQueryable ParseParenthesisQuery(ref JsonQueryReader reader)
    {
        reader.Read();

        IJsonQueryable query = ParseQueryCombination(ref reader);

        reader.Read();

        // Now should be end of parenthesis

        return query;
    }

    private static IJsonQueryable CreateOperatorQuery(string operatorName, IJsonQueryable leftQuery, IJsonQueryable rightQuery)
    {
        Type operatorType = OperatorRegistry.FindOperatorType(operatorName);

        return (IJsonQueryable)Activator.CreateInstance(operatorType, leftQuery, rightQuery);
    }
}