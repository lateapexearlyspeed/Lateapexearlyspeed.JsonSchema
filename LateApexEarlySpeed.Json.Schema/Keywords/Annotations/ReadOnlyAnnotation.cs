using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("readOnly")]
[JsonConverter(typeof(BooleanAnnotationJsonConverter<ReadOnlyAnnotation>))]
public class ReadOnlyAnnotation : AnnotationKeyword<bool>
{
}