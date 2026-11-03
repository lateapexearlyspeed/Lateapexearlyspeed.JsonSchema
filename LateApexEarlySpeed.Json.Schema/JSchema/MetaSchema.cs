using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

[JsonConverter(typeof(MetaSchemaJsonConverter))]
internal class MetaSchema
{
    public Uri Id { get; }
    private readonly Vocabulary[] _vocabularies;

    public MetaSchema(Uri id, Vocabulary[] vocabularies)
    {
        Id = id;
        _vocabularies = vocabularies;
    }

    public bool ContainsKeyword(ReadOnlySpan<char> keyword)
    {
        foreach (Vocabulary vocabulary in _vocabularies)
        {
            if (vocabulary.ContainsKeyword(keyword))
            {
                return true;
            }
        }

        return false;
    }

    public Type? GetKeyword(ReadOnlySpan<char> keyword)
    {
        foreach (Vocabulary vocabulary in _vocabularies)
        {
            Type? type = vocabulary.GetKeyword(keyword);
            if (type is not null)
            {
                return type;
            }
        }

        return null;
    }
}