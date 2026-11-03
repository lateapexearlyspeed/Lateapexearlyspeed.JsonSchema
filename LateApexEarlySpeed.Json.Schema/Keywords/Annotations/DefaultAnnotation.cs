using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword(Keyword)]
[JsonConverter(typeof(DefaultAnnotationJsonConverter))]
public class DefaultAnnotation : AnnotationKeywordBase
{
    public const string Keyword = "default";
}