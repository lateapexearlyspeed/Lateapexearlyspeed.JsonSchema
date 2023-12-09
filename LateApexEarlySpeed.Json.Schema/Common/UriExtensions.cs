namespace LateApexEarlySpeed.Json.Schema.Common;

internal static class UriExtensions
{
    public static string UnescapedFragmentWithoutNumberSign(this Uri uri)
    {
        string escapedFragmentWithoutNumberSign = uri.Fragment.TrimStart('#');
        return Uri.UnescapeDataString(escapedFragmentWithoutNumberSign);
    }
}