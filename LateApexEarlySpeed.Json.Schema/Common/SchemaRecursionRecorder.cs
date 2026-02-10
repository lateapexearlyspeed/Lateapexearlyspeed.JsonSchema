using System.Diagnostics;
using LateApexEarlySpeed.Json.Schema.JSchema;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class SchemaRecursionRecorder
{
    private readonly HashSet<(JsonSchema schema, LinkedListBasedImmutableJsonPointer instanceLocation)> _schemaInstancesHash = new();
    private readonly Stack<(JsonSchema schema, LinkedListBasedImmutableJsonPointer instanceLocation)> _schemaInstancesStack = new();

    /// <returns>If there was already <paramref name="schema"/> and <paramref name="instanceLocation"/> in recorder, return false</returns>
    public bool TryPushRecord(JsonSchema schema, LinkedListBasedImmutableJsonPointer instanceLocation)
    {
        bool canAdd = _schemaInstancesHash.Add((schema, instanceLocation));
        if (!canAdd)
        {
            return false;
        }
        
        _schemaInstancesStack.Push((schema, instanceLocation));

        return true;
    }

    public void PopRecord()
    {
        Debug.Assert(_schemaInstancesStack.Count != 0);
        (JsonSchema schema, LinkedListBasedImmutableJsonPointer instanceLocation) = _schemaInstancesStack.Pop();
        
        bool canFindFromStack = _schemaInstancesHash.Remove((schema, instanceLocation));
        Debug.Assert(canFindFromStack);
    }
}