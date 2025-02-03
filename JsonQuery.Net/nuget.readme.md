# JsonQuery.Net

This is [JsonQuery](https://jsonquerylang.org/) .Net implementation library, which passed official [test suite](https://github.com/jsonquerylang/jsonquery/blob/develop/test-suite/README.md).

---
## Compile json format query to query engine:

```csharp
IJsonQueryable queryable = JsonQueryable.Compile(jsonFormatQuery);
```

## Parse json query statement to query engine:

```csharp
IJsonQueryable queryable = JsonQueryable.Parse(jsonQuery);
```

## Execute query against json data by query engine:

```csharp
JsonNode? result = queryable.Query(jsonData);
```

## Serialize query engine itself to json format

```csharp
string jsonFormatQuery = queryable.SerializeToJsonFormat();
```

## More functionalities later.
