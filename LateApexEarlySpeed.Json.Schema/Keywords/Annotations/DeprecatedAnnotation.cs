using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword(Keyword)]
[JsonConverter(typeof(BooleanAnnotationJsonConverter<DeprecatedAnnotation>))]
public class DeprecatedAnnotation : AnnotationKeywordBase
{
    public const string Keyword = "deprecated";
}