using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

internal interface IJsonSchemaResourceNodesCleanable
{
    /// <summary>
    /// Remove all ids in <see cref="EmbeddedJsonSchemaResource"/> (replace <see cref="EmbeddedJsonSchemaResource"/> with <see cref="BodyJsonSchema"/>) for all children nodes, recursively.
    /// </summary>
    void RemoveIdFromAllChildrenSchemaElements();
}