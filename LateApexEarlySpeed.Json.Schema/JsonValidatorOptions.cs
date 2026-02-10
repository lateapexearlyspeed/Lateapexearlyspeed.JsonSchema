using LateApexEarlySpeed.Json.Schema.Keywords;

namespace LateApexEarlySpeed.Json.Schema;

public class JsonValidatorOptions
{
    /// <summary>
    /// Gets or sets a value that determines whether a property's name uses a case-insensitive comparison during validation. The default value is false.
    /// </summary>
    /// <returns>
    /// true to compare property names using case-insensitive comparison; otherwise, false.
    /// </returns>
    public bool PropertyNameCaseInsensitive { get; set; }

    /// <summary>
    /// Gets or sets a value that determines whether removing json schema resource id from unknown keywords. The default value is false.
    /// </summary>
    public bool IgnoreResourceIdInUnknownKeyword { set; get; }

    /// <summary>
    /// Gets or sets a value that determines default dialect when there is no '$schema' identifier in Json schema. The default value is <see cref="DialectKind.Draft202012"/>.
    /// </summary>
    public DialectKind DefaultDialect { get; set; }

    internal static JsonValidatorOptions Default { get; } = new();

    internal bool Equals(JsonValidatorOptions other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return PropertyNameCaseInsensitive == other.PropertyNameCaseInsensitive 
               && IgnoreResourceIdInUnknownKeyword == other.IgnoreResourceIdInUnknownKeyword
               && DefaultDialect == other.DefaultDialect;
    }
}
