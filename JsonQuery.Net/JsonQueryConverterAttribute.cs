namespace JsonQuery.Net;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class JsonQueryConverterAttribute : Attribute
{
    public Type ParserType { get; }

    public JsonQueryConverterAttribute(Type parserType)
    {
        ParserType = parserType;
    }
}