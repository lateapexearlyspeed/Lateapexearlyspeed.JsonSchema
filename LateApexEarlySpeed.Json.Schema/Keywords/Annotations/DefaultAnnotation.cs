using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword("default")]
[JsonConverter(typeof(DefaultAnnotationJsonConverter))]
public class DefaultAnnotation : AnnotationKeyword<JsonElement>
{
}