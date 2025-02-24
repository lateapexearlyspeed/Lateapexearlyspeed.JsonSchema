namespace JsonQuery.Net.Queryables.Linq;

public class OrderByQuery : SortQuery
{
    internal new const string Keyword = "orderBy";

    [QueryArgument(0)] 
    public new IJsonQueryable SubQuery => base.SubQuery;

    public OrderByQuery(IJsonQueryable keySelector) : base(keySelector)
    {
    }
}