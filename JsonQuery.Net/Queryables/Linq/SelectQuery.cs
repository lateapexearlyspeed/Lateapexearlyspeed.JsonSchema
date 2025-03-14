﻿namespace JsonQuery.Net.Queryables.Linq;

public class SelectQuery : MapQuery
{
    internal new const string Keyword = "select";

    [QueryParam(0)]
    public new IJsonQueryable SubQuery => base.SubQuery;

    public SelectQuery(IJsonQueryable itemQuery) : base(itemQuery)
    {
    }
}