using System.Text.Json;
using System.Text.Json.Nodes;
using JsonQuery.Net.Queryables;

namespace JsonQuery.Net;

public static class JsonQueryable
{
    internal static readonly JsonSerializerOptions JsonSerializerOptions = new() {Converters = { new DefaultJsonQueryableConverter() }};

    /// <summary>
    /// Compile "json format" query to <see cref="IJsonQueryable"/> query engine
    /// </summary>
    /// <param name="jsonFormatQuery">json query which is json format</param>
    /// <returns>The generated <see cref="IJsonQueryable"/> instance which can be used to query actual json data</returns>
    public static IJsonQueryable Compile(string jsonFormatQuery)
    {
        return JsonSerializer.Deserialize<IJsonQueryable>(jsonFormatQuery, JsonSerializerOptions)!;
    }

    /// <summary>
    /// Compile "json format" query to <see cref="IJsonQueryable"/> query engine
    /// </summary>
    /// <param name="jsonFormatQuery">json query which is json format</param>
    /// <returns>The generated <see cref="IJsonQueryable"/> instance which can be used to query actual json data</returns>
    public static IJsonQueryable Compile(JsonNode? jsonFormatQuery)
    {
        return jsonFormatQuery.Deserialize<IJsonQueryable>(JsonSerializerOptions)!;
    }

    /// <summary>
    /// Parse json query to <see cref="IJsonQueryable"/> query engine
    /// </summary>
    /// <param name="jsonQuery">The query which is human friendly syntax</param>
    /// <returns>The generated <see cref="IJsonQueryable"/> instance which can be used to query actual json data</returns>
    public static IJsonQueryable Parse(string jsonQuery)
    {
        JsonQueryReader reader = new JsonQueryReader(jsonQuery);

        reader.Read();
        IJsonQueryable jsonQueryable = JsonQueryParser.ParseQueryCombination(ref reader);

        return jsonQueryable;
    }
}