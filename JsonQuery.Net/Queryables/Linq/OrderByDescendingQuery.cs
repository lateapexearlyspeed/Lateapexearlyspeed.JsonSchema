namespace JsonQuery.Net.Queryables.Linq;

public class OrderByDescendingQuery : SortQuery
{
    internal new const string Keyword = "orderByDescending";

    [QueryArgument(0)] 
    public new IJsonQueryable SubQuery => base.SubQuery;

    public OrderByDescendingQuery(IJsonQueryable keySelector) : base(keySelector, true)
    {
    }
}