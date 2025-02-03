namespace JsonQuery.Net;

public abstract class JsonQueryConverter<TQuery> : IJsonQueryConverter where TQuery : IJsonQueryable
{
    public abstract TQuery Read(ref JsonQueryReader reader);

    IJsonQueryable IJsonQueryConverter.Read(ref JsonQueryReader reader)
    {
        int before = reader.PositionStackCount; // the count has already included recently pushed InFunction now
        
        TQuery query = Read(ref reader);

        if (reader.PositionStackCount != before - 1 || reader.TokenType != JsonQueryTokenType.EndParenthesis)
        {
            throw new JsonQueryParseException($"Error during json query parsing for {typeof(TQuery)}", reader.Position);
        }

        return query;
    }
}

internal interface IJsonQueryConverter
{
    IJsonQueryable Read(ref JsonQueryReader reader);
}