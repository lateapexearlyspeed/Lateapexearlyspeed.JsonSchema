namespace LateApexEarlySpeed.Json.Schema.Generator;

[AttributeUsage(AttributeTargets.Property)]
public class MinItemsAttribute : Attribute
{
    public uint MinItems { get; }

    public MinItemsAttribute(uint minItems)
    {
        MinItems = minItems;
    }
}