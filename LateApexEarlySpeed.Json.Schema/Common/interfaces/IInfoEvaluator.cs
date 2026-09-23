using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Common.interfaces;

public interface IInfoEvaluator<out TValue>
{
    IReadOnlyCollection<TValue> GetEvaluatedInfos(JsonInstanceElement instance, JsonSchemaOptions options);
}