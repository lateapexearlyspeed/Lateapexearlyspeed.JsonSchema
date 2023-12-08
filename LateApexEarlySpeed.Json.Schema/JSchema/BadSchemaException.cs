using System;

namespace LateApexEarlySpeed.Json.Schema.JSchema;

public class BadSchemaException : Exception
{
    public BadSchemaException(string message) : base(message)
    {
    }
}