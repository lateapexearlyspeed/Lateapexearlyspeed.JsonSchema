namespace JsonQuery.Net.Queryables.Linq;

public class LastOrDefaultQuery : LastQuery
{
    internal new const string Keyword = "lastOrDefault";

    public LastOrDefaultQuery(IJsonQueryable filter) : base(filter)
    {
    }
}