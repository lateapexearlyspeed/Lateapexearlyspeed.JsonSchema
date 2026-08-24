using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("contentMediaType")]
[JsonConverter(typeof(StringAnnotationJsonConverter<ContentMediaTypeAnnotation>))]
public class ContentMediaTypeAnnotation : AnnotationKeywordBase
{
}