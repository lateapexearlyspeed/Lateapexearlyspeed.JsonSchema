using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword(Keyword)]
[JsonConverter(typeof(StringAnnotationJsonConverter<ContentMediaTypeAnnotation>))]
public class ContentMediaTypeAnnotation : AnnotationKeywordBase
{
    public const string Keyword = "contentMediaType";

    public override bool ShouldAnnotate(JsonInstanceElement instance)
    {
        return instance.ValueKind == JsonValueKind.String;
    }
}