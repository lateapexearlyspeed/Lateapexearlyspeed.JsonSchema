namespace LateApexEarlySpeed.Json.Schema.Keywords;

[AttributeUsage(AttributeTargets.Class)]
public class DialectAttribute : Attribute
{
    public DialectKind[] Dialects { get; }

    public DialectAttribute(params DialectKind[] dialects)
    {
        Dialects = dialects;
    }
}