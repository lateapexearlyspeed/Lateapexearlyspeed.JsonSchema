using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.JInstance;

namespace LateApexEarlySpeed.Json.Schema.Keywords;

/// <summary>
/// Base type for keywords which apply their sub schema(s) to a <b>different</b> json instance than current one (eg: property values, array items, property names).
/// Such sub schemas start fresh 'unevaluated' scopes, so evaluation info produced inside them must not be attributed to the
/// 'unevaluatedProperties' / 'unevaluatedItems' keyword which is being evaluated for current instance.
/// </summary>
/// <remarks>
/// Keywords which apply sub schema(s) to the <b>same</b> json instance (eg: 'allOf', 'anyOf', '$ref') must NOT derive from this type - they propagate evaluation info upwards.
/// </remarks>
internal abstract class ResetUnevaluatedScopeKeywordBase : ValidationKeywordBase
{
    public sealed override ValidationResult Validate(JsonInstanceElement instance, JsonSchemaOptions options)
    {
        bool origPropState = options.EnterUnevaluatedPropertiesScope(false);
        bool origItemState = options.EnterUnevaluatedItemsScope(false);

        ValidationResult validationResult = base.Validate(instance, options);

        options.ExitUnevaluatedItemsScope(origItemState);
        options.ExitUnevaluatedPropertiesScope(origPropState);

        return validationResult;
    }
}