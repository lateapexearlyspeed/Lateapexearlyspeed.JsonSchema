namespace JsonQuery.Net.Queryables;

public interface ISingleSubQuery
{
    IJsonQueryable SubQuery { get; }
}