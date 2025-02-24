namespace JsonQuery.Net.Queryables.Linq;

public class TakeQuery : LimitQuery
{
    internal new const string Keyword = "take";

    [QueryArgument(0)]
    public int Count => LimitSize;

    public TakeQuery(int count) : base(count)
    {
    }
}