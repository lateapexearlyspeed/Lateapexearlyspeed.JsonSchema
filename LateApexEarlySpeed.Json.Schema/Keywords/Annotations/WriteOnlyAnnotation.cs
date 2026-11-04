using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword(Keyword)]
[JsonConverter(typeof(BooleanAnnotationJsonConverter<WriteOnlyAnnotation>))]
public class WriteOnlyAnnotation : AnnotationKeywordBase
{
    public const string Keyword = "writeOnly";
}