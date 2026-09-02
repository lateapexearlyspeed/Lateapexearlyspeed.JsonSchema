using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("description")]
[JsonConverter(typeof(StringAnnotationJsonConverter<DescriptionAnnotation>))]
public class DescriptionAnnotation : AnnotationKeywordBase
{
}