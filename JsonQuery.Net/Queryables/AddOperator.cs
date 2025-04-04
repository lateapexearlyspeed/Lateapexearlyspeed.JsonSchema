using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(QueryCollectionConverter))]
public class AddOperator : IJsonQueryable, IMultipleSubQuery
{
    internal const string Keyword = "add";
    internal const string Operator = "+";

    private readonly IJsonQueryable[] _operandQueries;

    public AddOperator(IJsonQueryable[] operandQueries)
    {
        if (operandQueries.Length < 2)
        {
            throw new ArgumentException($"Operands count of '{Operator}' operator should be greater than 1.", nameof(operandQueries));
        }

        _operandQueries = operandQueries;
    }

    public JsonNode? Query(JsonNode? data)
    {
        IEnumerable<JsonNode?> operandNodes = _operandQueries.Select(operandQuery => operandQuery.Query(data));

        var operandValues = new List<JsonValue>();

        foreach (JsonNode? operandNode in operandNodes)
        {
            if (operandNode is not JsonValue operandValue)
            {
                return null;
            }

            operandValues.Add(operandValue);
        }

        bool allDecimal = true;
        var decimalOperands = new decimal[operandValues.Count];

        for (int i = 0; i < operandValues.Count; i++)
        {
            if (!operandValues[i].TryGetValue(out decimal decimalValue))
            {
                allDecimal = false;
                break;
            }

            decimalOperands[i] = decimalValue;
        }

        if (allDecimal)
        {
            return decimalOperands.Sum();
        }

        if (operandValues.Any(operand => operand.GetValueKind() == JsonValueKind.String))
        {
            return string.Concat(operandValues);
        }

        return null;
    }

    public IEnumerable<IJsonQueryable> SubQueries => _operandQueries;
}