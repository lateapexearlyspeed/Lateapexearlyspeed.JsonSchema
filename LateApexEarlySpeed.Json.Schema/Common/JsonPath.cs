namespace LateApexEarlySpeed.Json.Schema.Common;

internal class JsonPath
{
    private readonly string _path;

    private JsonPath(string path)
    {
        _path = path;
    }

    public static JsonPath Root => new(string.Empty);

    public JsonPath AppendPathSegment(string pathSegment) => new(_path + "/" + pathSegment);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not JsonPath other)
        {
            return false;
        }

        return _path == other._path;
    }

    public override int GetHashCode()
    {
        return _path.GetHashCode();
    }
}