namespace JsonQuery.Net;

[AttributeUsage(AttributeTargets.Class)]
public class JsonQueryConverterAttribute : Attribute
{
    public Type ParserType { get; }

    public JsonQueryConverterAttribute(Type parserType)
    {
        ParserType = parserType;
    }
}