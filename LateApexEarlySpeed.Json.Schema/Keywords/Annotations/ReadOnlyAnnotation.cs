using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword(Keyword)]
[JsonConverter(typeof(BooleanAnnotationJsonConverter<ReadOnlyAnnotation>))]
public class ReadOnlyAnnotation : AnnotationKeywordBase
{
    public const string Keyword = "readOnly";
}