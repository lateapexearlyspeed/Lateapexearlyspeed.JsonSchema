namespace JsonSchemaConsoleApp;

public class JsonPointer
{
    private readonly List<string> _segments;

    public JsonPointer(string jsonPointerPath)
    {
        _segments = jsonPointerPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public JsonPointer(IEnumerable<string> collection)
    {
        _segments = collection.ToList();
    }

    public int Count => _segments.Count;

    public string GetSegment(int index)
    {
        return _segments[index];
    }

    public void AddSegment(string segment)
    {
        _segments.Add(segment);
    }

    public override string ToString()
    {
        return "/" + string.Join('/', _segments);
    }
}