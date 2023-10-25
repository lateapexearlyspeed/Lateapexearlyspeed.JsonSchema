using System.Collections;
using System.Collections.Immutable;

namespace JsonSchemaConsoleApp;

public class ReferencePath : IEnumerable<JsonSchemaResource>
{
    private readonly ImmutableArray<JsonSchemaResource> _path;

    public static ReferencePath CreateRoot(JsonSchemaResource root)
    {
        return new ReferencePath(ImmutableArray.Create(root));
    }

    private ReferencePath(ImmutableArray<JsonSchemaResource> path)
    {
        _path = path;
    }

    public ReferencePath Append(JsonSchemaResource schemaResource)
    {
        return new ReferencePath(_path.Add(schemaResource));
    }

    public IEnumerator<JsonSchemaResource> GetEnumerator()
    {
        return ((IEnumerable<JsonSchemaResource>)_path).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}