namespace LateApexEarlySpeed.Json.Schema.Keywords;

[AttributeUsage(AttributeTargets.Class)]
internal class FormatAttribute : Attribute
{
    public string Name { get; }

    public FormatAttribute(string name)
    {
        Name = name;
    }
}