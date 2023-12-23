namespace LateApexEarlySpeed.Json.Schema.Keywords;

[AttributeUsage(AttributeTargets.Class)]
public class FormatAttribute : Attribute
{
    public string Name { get; }

    public FormatAttribute(string name)
    {
        Name = name;
    }
}