using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JsonQuery.Net.Queryables;

[JsonConverter(typeof(QueryCollectionConverter))]
public class OrQuery : IJsonQueryable, IMultipleSubQuery
{
    internal const string Keyword = "or";
    internal const string Operator = "or";

    private readonly IJsonQueryable[] _operandQueries;

    public OrQuery(IJsonQueryable[] operandQueries)
    {
        if (operandQueries.Length < 2)
        {
            throw new ArgumentException($"Operands count of '{Operator}' operator should be greater than 1.", nameof(operandQueries));
        }

        _operandQueries = operandQueries;
    }

    public JsonNode Query(JsonNode? data)
    {
        IEnumerable<bool> operands = _operandQueries.Select(operandQuery => operandQuery.Query(data).GetBooleanValue());

        return operands.Any(operand => operand);
    }

    public IEnumerable<IJsonQueryable> SubQueries => _operandQueries;
}