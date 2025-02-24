namespace JsonQuery.Net;

[AttributeUsage(AttributeTargets.Property)]
public class QueryArgumentAttribute : Attribute
{
    public int Index { get; }

    public QueryArgumentAttribute(int index)
    {
        Index = index;
    }
}