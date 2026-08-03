using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("writeOnly")]
[JsonConverter(typeof(BooleanAnnotationJsonConverter<WriteOnlyAnnotation>))]
public class WriteOnlyAnnotation : AnnotationKeyword<bool>
{
}