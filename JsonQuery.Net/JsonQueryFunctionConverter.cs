using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

public abstract class JsonQueryFunctionConverter<TQuery> : IJsonQueryConverter where TQuery : IJsonQueryable
{
    protected static readonly string QueryKeyword = JsonQueryableRegistry.GetKeyword(typeof(TQuery));

    /// <summary>
    /// Should be implemented to read to end of parenthesis
    /// </summary>
    protected abstract TQuery ReadArguments(ref JsonQueryReader reader);

    IJsonQueryable IJsonQueryConverter.Read(ref JsonQueryReader reader)
    {
        int before = reader.PositionStackCount; // the count has already included recently pushed InFunction now

        reader.Read(); // skip FunctionName
        reader.Read(); // skip StartParenthesis

        TQuery query = ReadArguments(ref reader);

        if (reader.PositionStackCount != before - 1 || reader.TokenType != JsonQueryTokenType.EndParenthesis)
        {
            throw new JsonQueryParseException($"Error during json query parsing for '{QueryKeyword}'", reader.Position);
        }

        return query;
    }

    public virtual bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(TQuery);
    }
}

public interface IJsonQueryConverter : IJsonQueryTypeChecker
{
    IJsonQueryable Read(ref JsonQueryReader reader);
}

public interface IJsonQueryConverterFactory : IJsonQueryTypeChecker
{
    IJsonQueryConverter CreateConverter(Type typeToConvert);
}

public interface IJsonQueryTypeChecker
{
    bool CanConvert(Type typeToConvert);
}