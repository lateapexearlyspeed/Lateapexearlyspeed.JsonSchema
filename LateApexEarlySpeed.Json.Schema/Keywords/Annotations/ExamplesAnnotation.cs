using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword(Keyword)]
[JsonConverter(typeof(ExamplesAnnotationJsonConverter))]
public class ExamplesAnnotation : AnnotationKeywordBase
{
    public const string Keyword = "examples";
}