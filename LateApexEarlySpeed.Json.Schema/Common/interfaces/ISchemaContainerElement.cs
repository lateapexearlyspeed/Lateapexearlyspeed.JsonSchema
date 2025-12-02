using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

public interface ISchemaContainerElement
{
    /// <summary>
    /// Find sub container element by name
    /// </summary>
    /// <param name="name"></param>
    /// <remarks>if cannot find out specified element, should return null rather than throw exception.</remarks>
    /// <returns>null if cannot find out</returns>
    ISchemaContainerElement? GetSubElement(string name);
    bool IsSchemaType { get; }
    JsonSchema GetSchema();
    IEnumerable<ISchemaContainerElement> EnumerateElements();
}