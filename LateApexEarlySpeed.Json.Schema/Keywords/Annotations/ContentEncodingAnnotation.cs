using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("contentEncoding")]
[JsonConverter(typeof(StringAnnotationJsonConverter<ContentEncodingAnnotation>))]
public class ContentEncodingAnnotation : AnnotationKeywordBase
{
}