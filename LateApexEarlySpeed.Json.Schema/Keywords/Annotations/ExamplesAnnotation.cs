using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("examples")]
[JsonConverter(typeof(ExamplesAnnotationJsonConverter))]
public class ExamplesAnnotation : AnnotationKeywordBase
{
}