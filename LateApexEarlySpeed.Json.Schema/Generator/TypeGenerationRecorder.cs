using System.Diagnostics;

namespace LateApexEarlySpeed.Json.Schema.Generator;

internal class TypeGenerationRecorder
{
    private readonly Stack<Type> _stack = new();
    private readonly HashSet<Type> _hash = new();

    public void PushType(Type type)
    {
        if (!_hash.Add(type))
        {
            throw new InvalidOperationException($"Loop reference detected. Type: {type}");
        }

        _stack.Push(type);
    }

    public void PopType()
    {
        Debug.Assert(_stack.Count != 0);
        Type poppedType = _stack.Pop();

        bool existsInHash = _hash.Remove(poppedType);
        Debug.Assert(existsInHash);
    }
}