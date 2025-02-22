﻿using System.Text.Json.Nodes;

namespace JsonQuery.Net.Queryables.Linq;

public class AppendQuery : IJsonQueryable
{
    internal const string Keyword = "append";

    public IJsonQueryable ArrayQuery { get; }
    public IJsonQueryable AppendedElementQuery { get; }

    public AppendQuery(IJsonQueryable arrayQuery, IJsonQueryable appendedElementQuery)
    {
        ArrayQuery = arrayQuery;
        AppendedElementQuery = appendedElementQuery;
    }

    public JsonNode? Query(JsonNode? data)
    {
        if (ArrayQuery.Query(data) is not JsonArray array)
        {
            return null;
        }

        IEnumerable<JsonNode?> result = array.Append(AppendedElementQuery.Query(data)?.DeepClone());

        return new JsonArray(result.ToArray());
    }
}