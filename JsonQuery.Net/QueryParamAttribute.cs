namespace JsonQuery.Net;

[AttributeUsage(AttributeTargets.Property)]
public class QueryParamAttribute : Attribute
{
    public int Index { get; }

    public QueryParamAttribute(int index)
    {
        Index = index;
    }
}