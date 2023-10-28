namespace JsonSchemaConsoleApp.Keywords.interfaces;

internal static class SubSchemaCollectionExtensions
{
    public static ISchemaContainerElement? GetSubElement(this ISubSchemaCollection subSchemaCollection, string name)
    {
        return uint.TryParse(name, out uint idx) && idx < subSchemaCollection.SubSchemas.Count
            ? subSchemaCollection.SubSchemas[(int)idx]
            : null;
    }

    public static IEnumerable<ISchemaContainerElement> EnumerateElements(this ISubSchemaCollection subSchemaCollection)
    {
        return subSchemaCollection.SubSchemas;
    }
}