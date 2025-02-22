namespace JsonQuery.Net.Queryables.Linq;

public class FirstOrDefaultQuery : FirstQuery
{
    internal new const string Keyword = "firstOrDefault";

    public FirstOrDefaultQuery(IJsonQueryable filter) : base(filter)
    {
    }
}