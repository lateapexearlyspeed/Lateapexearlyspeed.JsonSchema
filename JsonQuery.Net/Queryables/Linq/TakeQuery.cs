namespace JsonQuery.Net.Queryables.Linq;

public class TakeQuery : LimitQuery
{
    internal new const string Keyword = "take";

    public TakeQuery(int count) : base(count)
    {
    }
}