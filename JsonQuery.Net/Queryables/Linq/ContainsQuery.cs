namespace JsonQuery.Net.Queryables.Linq;

public class ContainsQuery : AnyQuery
{
    internal new const string Keyword = "contains";

    public ContainsQuery(IJsonQueryable query) : base(query)
    {
    }
}