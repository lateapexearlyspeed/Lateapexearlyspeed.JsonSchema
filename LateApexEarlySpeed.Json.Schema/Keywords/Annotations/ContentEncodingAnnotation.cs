using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("contentEncoding")]
[JsonConverter(typeof(StringAnnotationJsonConverter<ContentEncodingAnnotation>))]
public class ContentEncodingAnnotation : AnnotationKeywordBase
{
    public override bool ShouldAnnotate(JsonInstanceElement instance)
    {
        return instance.ValueKind == JsonValueKind.String;
    }
}