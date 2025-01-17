namespace JsonQuery.Net;

public abstract class JsonQueryConverter<TQuery> : IJsonQueryConverter where TQuery : IJsonQueryable
{
    public abstract TQuery Read(ref JsonQueryReader reader);

    IJsonQueryable IJsonQueryConverter.Read(ref JsonQueryReader reader)
    {
        return Read(ref reader);
    }
}

internal interface IJsonQueryConverter
{
    IJsonQueryable Read(ref JsonQueryReader reader);
}