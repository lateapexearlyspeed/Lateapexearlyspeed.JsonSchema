namespace JsonQuery.Net.Queryables.Linq;

public class WhereQuery : FilterQuery
{
    internal new const string Keyword = "where";

    [QueryArgument(0)] 
    public IJsonQueryable PredicateQuery => SubQuery;

    public WhereQuery(IJsonQueryable filter) : base(filter)
    {
    }
}