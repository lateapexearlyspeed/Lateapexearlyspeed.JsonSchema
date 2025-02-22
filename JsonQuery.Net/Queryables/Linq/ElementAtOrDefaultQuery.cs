namespace JsonQuery.Net.Queryables.Linq;

public class ElementAtOrDefaultQuery : ElementAtQuery
{
    internal new const string Keyword = "elementAtOrDefault";

    public ElementAtOrDefaultQuery(int index) : base(index)
    {
    }
}