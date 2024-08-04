using LateApexEarlySpeed.Json.Schema.Common;
using Xunit;

namespace LateApexEarlySpeed.Json.Schema.UnitTests;

public class GlobalRegexCacheTests
{
    private const int ConcurrencyRead = 10;

    [Fact]
    public async Task TestPatternAsync()
    {
        var regexCache = new GlobalRegexCache();

        await RunPerCacheSizeWithDifferentPatternsAsync(regexCache, regexCache.CacheSize);
        await RunPerCacheSizeWithDifferentPatternsAsync(regexCache, regexCache.CacheSize * 2);
        await RunPerCacheSizeWithDifferentPatternsAsync(regexCache, regexCache.CacheSize * 2);

        await RunPerCacheSizeWithDifferentPatternsAsync(regexCache, regexCache.CacheSize / 2);
        await RunPerCacheSizeWithDifferentPatternsAsync(regexCache, regexCache.CacheSize / 2);
    }

    private static async Task RunPerCacheSizeWithDifferentPatternsAsync(GlobalRegexCache regexCache, int cacheSize)
    {
        regexCache.CacheSize = cacheSize;

        for (int i = 0; i < cacheSize * 3; i++) // To run case: query samples exceeds cache size.
        {
            string pattern = i.ToString();

            IEnumerable<Task<LazyCompiledRegex>> tasks = Enumerable.Range(0, ConcurrencyRead)
                .Select(_ => Task.Run(() => regexCache.Get(pattern, RegexFactory.DefaultMatchTimeout)));

            LazyCompiledRegex[] regexList = await Task.WhenAll(tasks);
            Assert.Single(regexList.Distinct()); // To verify same pattern string will generate singleton regex
            Assert.True(regexList.Distinct().Single().IsMatch(pattern));
        }
    }

    [Fact]
    public async Task TestMatchTimeoutAsync()
    {
        var regexCache = new GlobalRegexCache();

        await RunPerCacheSizeWithDifferentMatchTimeoutsAsync(regexCache, regexCache.CacheSize);
        await RunPerCacheSizeWithDifferentMatchTimeoutsAsync(regexCache, regexCache.CacheSize * 2);
        await RunPerCacheSizeWithDifferentMatchTimeoutsAsync(regexCache, regexCache.CacheSize * 2);

        await RunPerCacheSizeWithDifferentMatchTimeoutsAsync(regexCache, regexCache.CacheSize / 2);
        await RunPerCacheSizeWithDifferentMatchTimeoutsAsync(regexCache, regexCache.CacheSize / 2);
    }

    private static async Task RunPerCacheSizeWithDifferentMatchTimeoutsAsync(GlobalRegexCache regexCache, int cacheSize)
    {
        regexCache.CacheSize = cacheSize;
        string pattern = "abc";
        for (int i = 1; i < cacheSize * 3; i++) // To run case: query samples exceeds cache size.
        {
            TimeSpan matchTimeout = TimeSpan.FromMilliseconds(i);

            IEnumerable<Task<LazyCompiledRegex>> tasks = Enumerable.Range(0, ConcurrencyRead)
                .Select(_ => Task.Run(() => regexCache.Get(pattern, matchTimeout)));

            LazyCompiledRegex[] regexList = await Task.WhenAll(tasks);
            Assert.Single(regexList.Distinct()); // To verify same matchTimeout will generate singleton regex
            Assert.True(regexList.Distinct().Single().IsMatch(pattern));
        }
    }

    [Fact]
    public void Get_DifferentMatchTimeout_ReturnDifferentRegex()
    {
        const string pattern = "abc";
        var regexCache = new GlobalRegexCache();
        
        LazyCompiledRegex regex1 = regexCache.Get(pattern, TimeSpan.FromMilliseconds(1));
        LazyCompiledRegex regex2 = regexCache.Get(pattern, TimeSpan.FromMilliseconds(1));
        Assert.Same(regex1, regex2);
        
        LazyCompiledRegex regex3 = regexCache.Get(pattern, TimeSpan.FromMilliseconds(2));
        Assert.NotSame(regex1, regex3);
    }
}