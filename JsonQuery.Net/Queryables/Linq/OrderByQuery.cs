namespace JsonQuery.Net.Queryables.Linq;

public class OrderByQuery : SortQuery
{
    internal new const string Keyword = "orderBy";

    public OrderByQuery(IJsonQueryable keySelector) : base(keySelector)
    {
    }
}