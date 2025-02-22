namespace JsonQuery.Net.Queryables.Linq;

public class WhereQuery : FilterQuery
{
    internal new const string Keyword = "where";

    public WhereQuery(IJsonQueryable filter) : base(filter)
    {
    }
}