using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema.JSchema.JsonConverters;

internal class MetaSchemaJsonConverter : JsonConverter<MetaSchema>
{
    public const string VocabularyKeywordName = "$vocabulary";

    public override MetaSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateMetaSchemaHasInvalidJsonValueKindJsonException();
        }

        reader.Read();

        Uri? id = null;
        Vocabulary[]? vocabularies = null;

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.ValueTextEquals(IdKeyword.Keyword))
            {
                reader.Read();
                id = new Uri(reader.GetString()!);
            }
            else if (reader.ValueTextEquals(VocabularyKeywordName))
            {
                reader.Read();
                vocabularies = DeserializeVocabularies(ref reader);
            }
            else
            {
                reader.HandleFinalBlockAndSkip();
            }

            reader.Read();
        }

        if (id is null)
        {
            throw ThrowHelper.CreateMetaSchemaMissingIdJsonException();
        }

        return new MetaSchema(id, vocabularies ?? Array.Empty<Vocabulary>());
    }

    private static Vocabulary[] DeserializeVocabularies(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw ThrowHelper.CreateVocabularyHasInvalidJsonValueKindJsonException();
        }

        var vocabularies = new List<Vocabulary>();
        
        reader.Read();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            string vocabularyValue = reader.GetString()!;
            Vocabulary? vocabulary = Vocabulary.GetVocabulary(new Uri(vocabularyValue));
            if (vocabulary is not null)
            {
                vocabularies.Add(vocabulary);
            }

            reader.HandleFinalBlockAndSkip();
            reader.Read();
        }

        return vocabularies.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, MetaSchema value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override bool HandleNull => true;
}