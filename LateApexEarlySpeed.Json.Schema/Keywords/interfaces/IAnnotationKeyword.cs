using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;
using LateApexEarlySpeed.Json.Schema.Keywords.Annotations;

namespace LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

public interface IAnnotationKeyword
{
    string Name { get; }
    bool ShouldAnnotate(JsonInstanceElement instance);
    Annotation Annotate(JsonInstanceElement instance, JsonSchemaOptions options);
}