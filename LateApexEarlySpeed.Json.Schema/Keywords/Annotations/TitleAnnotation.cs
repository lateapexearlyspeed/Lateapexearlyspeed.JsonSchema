using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("title")]
[JsonConverter(typeof(StringAnnotationJsonConverter<TitleAnnotation>))]
public class TitleAnnotation : AnnotationKeyword<string>
{
}