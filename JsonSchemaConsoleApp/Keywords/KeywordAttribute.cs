namespace JsonSchemaConsoleApp.Keywords;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class KeywordAttribute : Attribute
{
    public string Name { get; }

    public KeywordAttribute(string name)
    {
        Name = name;
    }
}