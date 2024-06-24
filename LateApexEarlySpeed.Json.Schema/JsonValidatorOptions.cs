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

    internal static JsonValidatorOptions Default { get; } = new();

    internal bool Equals(JsonValidatorOptions other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return PropertyNameCaseInsensitive == other.PropertyNameCaseInsensitive;
    }
}