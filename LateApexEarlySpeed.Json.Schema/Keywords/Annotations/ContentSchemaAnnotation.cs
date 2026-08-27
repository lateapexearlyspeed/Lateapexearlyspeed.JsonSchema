using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations.JsonConverters;

namespace LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

[Keyword(Keyword)]
[JsonConverter(typeof(ContentSchemaAnnotationJsonConverter))]
public class ContentSchemaAnnotation : AnnotationKeywordBase
{
    public const string Keyword = "contentSchema";

    public override bool ShouldAnnotate(JsonInstanceElement instance)
    {
        return instance.ValueKind == JsonValueKind.String;
    }
}