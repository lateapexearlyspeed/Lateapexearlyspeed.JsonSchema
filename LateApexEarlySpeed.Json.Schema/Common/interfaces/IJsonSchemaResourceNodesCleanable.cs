using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

internal interface IJsonSchemaResourceNodesCleanable
{
    /// <summary>
    /// Remove all ids in <see cref="JsonSchemaResource"/> (replace <see cref="JsonSchemaResource"/> with <see cref="BodyJsonSchema"/>) for all children nodes, recursively.
    /// </summary>
    void RemoveIdFromAllChildrenSchemaElements();
}