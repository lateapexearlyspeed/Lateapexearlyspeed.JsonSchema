namespace JsonQuery.Net.Queryables;

public interface IMultipleSubQuery
{
    IEnumerable<IJsonQueryable> SubQueries { get; }
}