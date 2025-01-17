namespace JsonQuery.Net;

[AttributeUsage(AttributeTargets.Class)]
internal class JsonQueryConverterAttribute : Attribute
{
    public Type ParserType { get; }

    public JsonQueryConverterAttribute(Type parserType)
    {
        ParserType = parserType;
    }
}