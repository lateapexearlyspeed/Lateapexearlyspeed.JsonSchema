namespace JsonQuery.Net.Queryables.Linq;

public class OrderByDescendingQuery : SortQuery
{
    internal new const string Keyword = "orderByDescending";

    public OrderByDescendingQuery(IJsonQueryable keySelector) : base(keySelector, true)
    {
    }
}