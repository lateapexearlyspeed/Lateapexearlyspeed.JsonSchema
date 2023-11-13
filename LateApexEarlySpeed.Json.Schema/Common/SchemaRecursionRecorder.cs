using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class SchemaRecursionRecorder
{
    private readonly HashSet<(JsonSchema schema, JsonPath instancePath)> _schemaInstancesHash = new();
    private readonly Stack<(JsonSchema schema, JsonPath instancePath)> _schemaInstancesStack = new();

    /// <returns>If there was already <paramref name="schema"/> & <paramref name="instancePath"/> in recorder, return false</returns>
    public bool TryPushRecord(JsonSchema schema, JsonPath instancePath)
    {
        bool canAdd = _schemaInstancesHash.Add((schema, instancePath));
        if (!canAdd)
        {
            return false;
        }

        _schemaInstancesStack.Push((schema, instancePath));

        return true;
    }

    public void PopRecord()
    {
        Debug.Assert(_schemaInstancesStack.Count != 0);
        (JsonSchema schema, JsonPath instancePath) = _schemaInstancesStack.Pop();

        bool canFindFromStack = _schemaInstancesHash.Remove((schema, instancePath));
        Debug.Assert(canFindFromStack);
    }
}