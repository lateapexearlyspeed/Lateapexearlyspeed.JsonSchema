namespace JsonQuery.Net.Queryables.Linq;

public class SelectQuery : MapQuery
{
    internal new const string Keyword = "select";

    public SelectQuery(IJsonQueryable itemQuery) : base(itemQuery)
    {
    }
}