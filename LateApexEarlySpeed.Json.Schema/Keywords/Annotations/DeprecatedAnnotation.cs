using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("deprecated")]
[JsonConverter(typeof(BooleanAnnotationJsonConverter<DeprecatedAnnotation>))]
public class DeprecatedAnnotation : AnnotationKeyword<bool>
{
}