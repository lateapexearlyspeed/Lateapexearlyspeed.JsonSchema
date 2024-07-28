using LateApexEarlySpeed.Json.Schema.Common;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class GlobalRegexCacheTests
{
    private const int ConcurrencyRead = 10;

    [Fact]
    public async Task GetTestAsync()
    {
        var regexCache = new GlobalRegexCache();

        await RunPerCacheSizeAsync(regexCache, regexCache.CacheSize);
        await RunPerCacheSizeAsync(regexCache, regexCache.CacheSize * 2);
        await RunPerCacheSizeAsync(regexCache, regexCache.CacheSize * 2);

        await RunPerCacheSizeAsync(regexCache, regexCache.CacheSize / 2);
        await RunPerCacheSizeAsync(regexCache, regexCache.CacheSize / 2);
    }

    private static async Task RunPerCacheSizeAsync(GlobalRegexCache regexCache, int cacheSize)
    {
        regexCache.CacheSize = cacheSize;

        for (int i = 0; i < cacheSize * 3; i++) // To run case: query samples exceeds cache size.
        {
            string pattern = i.ToString();

            IEnumerable<Task<LazyCompiledRegex>> tasks = Enumerable.Range(0, ConcurrencyRead)
                .Select(_ => Task.Run(() => regexCache.Get(pattern)));

            LazyCompiledRegex[] regexList = await Task.WhenAll(tasks);
            Assert.Single(regexList.Distinct()); // To verify same pattern string will generate singleton regex
            Assert.True(regexList.First().IsMatch(pattern));
        }
    }
}