using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace LateApexEarlySpeed.Json.Schema.Common;

internal class HttpJsonDocumentClient : IAsyncDisposable
{
    private const string HttpClientName = "lateapexearlyspeed";

    private readonly ConcurrentDictionary<Uri, CacheItem> _httpDocumentsCache = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceProvider _sp;

    internal const int DefaultCacheExpirationHours = 1;
    private long _cacheExpirationTicks = TimeSpan.FromHours(DefaultCacheExpirationHours).Ticks;

    public HttpJsonDocumentClient()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(HttpClientName);
        _sp = services.BuildServiceProvider();
        _httpClientFactory = _sp.GetRequiredService<IHttpClientFactory>();
    }

    public async Task<string> GetDocumentAsync(Uri remoteUri)
    {
        if (_httpDocumentsCache.TryGetValue(remoteUri, out CacheItem cacheItem) && !cacheItem.IsExpired)
        {
            return cacheItem.Document;
        }

        string document = await RequestHttpDocumentAsync(remoteUri);

        _httpDocumentsCache[remoteUri] = new CacheItem(document, this);

        return document;
    }

    private async Task<string> RequestHttpDocumentAsync(Uri remoteUri)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient(HttpClientName);
        HttpResponseMessage response = await httpClient.GetAsync(remoteUri);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public TimeSpan CacheAbsoluteExpirationTime
    {
        get => TimeSpan.FromTicks(Interlocked.Read(ref _cacheExpirationTicks));
        set => Interlocked.Exchange(ref _cacheExpirationTicks, value.Ticks);
    }

    public ValueTask DisposeAsync() => _sp.DisposeAsync();

    private readonly struct CacheItem
    {
        private readonly HttpJsonDocumentClient _documentClient;
        private readonly DateTimeOffset _createTimeUtc;

        public CacheItem(string document, HttpJsonDocumentClient documentClient)
        {
            _documentClient = documentClient;
            Document = document;
            _createTimeUtc = DateTimeOffset.UtcNow;
        }

        public string Document { get; }

        public bool IsExpired => _createTimeUtc + _documentClient.CacheAbsoluteExpirationTime < DateTimeOffset.UtcNow;
    }
}