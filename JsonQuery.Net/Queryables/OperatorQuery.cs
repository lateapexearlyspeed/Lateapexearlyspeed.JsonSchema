using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables;

public abstract class OperatorQuery : IJsonQueryable
{
    public IJsonQueryable Left { get; }
    public IJsonQueryable Right { get; }

    protected OperatorQuery(IJsonQueryable left, IJsonQueryable right)
    {
        Left = left;
        Right = right;
    }

    protected decimal QueryLeftDecimal(JsonNode? data) => QueryDecimal(Left, data);
    protected decimal QueryRightDecimal(JsonNode? data) => QueryDecimal(Right, data);

    private static decimal QueryDecimal(IJsonQueryable query, JsonNode? data)
    {
        JsonNode? jsonNode = query.Query(data);
        if (jsonNode is null || jsonNode.GetValueKind() != JsonValueKind.Number)
        {
            return 0;
        }

        return jsonNode.GetValue<decimal>();
    }

    protected bool QueryLeftBoolean(JsonNode? data) => QueryBoolean(Left, data);
    protected bool QueryRightBoolean(JsonNode? data) => QueryBoolean(Right, data);

    private static bool QueryBoolean(IJsonQueryable query, JsonNode? data)
    {
        JsonNode? jsonNode = query.Query(data);

        return jsonNode.GetBooleanValue();
    }

    public abstract JsonNode? Query(JsonNode? data);
}

public class OperatorConverter<TOperator> : JsonFormatQueryJsonConverter<TOperator> where TOperator : OperatorQuery
{
    protected override TOperator ReadArguments(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IJsonQueryable left = JsonSerializer.Deserialize<IJsonQueryable>(ref reader, options)!;

        reader.Read();

        IJsonQueryable right = JsonSerializer.Deserialize<IJsonQueryable>(ref reader, options)!;

        reader.Read();

        return (TOperator)Activator.CreateInstance(typeof(TOperator), left, right);
    }

    public override void Write(Utf8JsonWriter writer, TOperator value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.GetKeyword());
        JsonSerializer.Serialize(writer, value.Left, options);
        JsonSerializer.Serialize(writer, value.Right, options);
        writer.WriteEndArray();
    }
}