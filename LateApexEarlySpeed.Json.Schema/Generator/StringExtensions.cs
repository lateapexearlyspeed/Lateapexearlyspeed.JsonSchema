namespace LateApexEarlySpeed.Json.Schema.Generator;

internal static class StringExtensions
{
    public static List<ReadOnlyMemory<char>> SplitWords(this string name)
    {
        if (name.Length == 0)
        {
            return new List<ReadOnlyMemory<char>>(0);
        }

        var words = new List<ReadOnlyMemory<char>>();

        int wordStartIdx = 0;

        while (wordStartIdx < name.Length)
        {
            int currentIdx = wordStartIdx + 1;

            for (; currentIdx < name.Length; currentIdx++)
            {
                if (char.IsUpper(name, currentIdx))
                {
                    break;
                }
            }

            words.Add(name.AsMemory(wordStartIdx, currentIdx - wordStartIdx));

            wordStartIdx = currentIdx;
        }

        return words;
    }
}