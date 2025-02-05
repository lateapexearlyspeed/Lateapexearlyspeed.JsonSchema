using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(OperatorConverter<AddOperator>))]
public class AddOperator : OperatorQuery
{
    internal const string Keyword = "add";
    internal const string Operator = "+";

    public AddOperator(IJsonQueryable left, IJsonQueryable right) : base(left, right)
    {
    }

    public override JsonNode? Query(JsonNode? data)
    {
        JsonNode? leftNode = Left.Query(data);
        JsonNode? rightNode = Right.Query(data);

        if (leftNode is not JsonValue leftValue || rightNode is not JsonValue rightValue)
        {
            return null;
        }

        if (leftValue.TryGetValue(out decimal leftNumber) && rightValue.TryGetValue(out decimal rightNumber))
        {
            return leftNumber + rightNumber;
        }

        if (leftValue.GetValueKind() == JsonValueKind.String || rightValue.GetValueKind() == JsonValueKind.String)
        {
            return leftValue.ToString() + rightValue;
        }

        return null;
    }
}