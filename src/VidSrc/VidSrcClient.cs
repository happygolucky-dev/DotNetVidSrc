using System.Net.Http.Json;
using VidSrc.Models;

namespace VidSrc;

/// <summary>Thrown when a VidSrc endpoint returns an unexpected (non-2xx, non-404) status code.</summary>
public sealed class VidSrcApiException : HttpRequestException
{
    /// <summary>Creates the exception for a failed request.</summary>
    public VidSrcApiException(System.Net.HttpStatusCode statusCode, string url)
        : base($"VidSrc request to '{url}' failed with status {(int)statusCode} ({statusCode}).", null, statusCode)
    {
    }
}

/// <summary>
/// API wrapper for VidSrc (<see href="https://vidsrc.sh/vidsrc/docs/">docs</see>).
/// No API key required.
/// <para>
/// All <c>id</c> arguments accept either an IMDB id (<c>tt0944947</c>)
/// or a numeric TMDB id (<c>1399</c>) — VidSrc detects the format.
/// </para>
/// </summary>
public sealed class VidSrcClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;

    /// <summary>Embed URL builder bound to the same base URL as this client.</summary>
    public VidSrcEmbed Embed { get; }

    /// <summary>Creates a client with its own internal <see cref="HttpClient"/>.</summary>
    public VidSrcClient(VidSrcClientOptions? options = null) : this(new HttpClient(), options, ownsHttpClient: true)
    {
    }

    /// <summary>Creates a client over a caller-supplied (e.g. <c>IHttpClientFactory</c>) <see cref="HttpClient"/>.</summary>
    public VidSrcClient(HttpClient httpClient, VidSrcClientOptions? options = null)
        : this(httpClient, options, ownsHttpClient: false)
    {
    }

    private VidSrcClient(HttpClient httpClient, VidSrcClientOptions? options, bool ownsHttpClient)
    {
        _http = httpClient;
        _ownsHttpClient = ownsHttpClient;
        _http.BaseAddress ??= new Uri((options?.BaseUrl ?? VidSrcClientOptions.DefaultBaseUrl).TrimEnd('/') + '/');
        Embed = new VidSrcEmbed(_http.BaseAddress.ToString());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_ownsHttpClient)
            _http.Dispose();
    }

    // ── Info endpoints ──────────────────────────────────────────────────────

    /// <summary><c>GET /info/movie/{id}.json</c>. Check <see cref="ContentInfo.IsAvailable"/> for availability.</summary>
    public Task<MovieInfo?> GetMovieInfoAsync(string id, CancellationToken cancellationToken = default)
        => GetJsonAsync<MovieInfo>($"info/movie/{Uri.EscapeDataString(id)}.json", allowNotFound: true, cancellationToken);

    /// <summary><c>GET /info/tv/{id}.json</c>.</summary>
    public Task<TvShowInfo?> GetTvShowInfoAsync(string id, CancellationToken cancellationToken = default)
        => GetJsonAsync<TvShowInfo>($"info/tv/{Uri.EscapeDataString(id)}.json", allowNotFound: true, cancellationToken);

    /// <summary><c>GET /info/tv/{id}/{season}/{episode}.json</c>.</summary>
    public Task<EpisodeInfo?> GetEpisodeInfoAsync(string id, int season, int episode, CancellationToken cancellationToken = default)
        => GetJsonAsync<EpisodeInfo>($"info/tv/{Uri.EscapeDataString(id)}/{season}/{episode}.json", allowNotFound: true, cancellationToken);

    // ── Latest content (paginated, 50 items per page) ───────────────────────

    /// <summary><c>GET /movies/latest/page-{page}.json</c>.</summary>
    public Task<LatestResult<LatestMovieItem>?> GetLatestMoviesAsync(int page = 1, CancellationToken cancellationToken = default)
        => GetLatestAsync<LatestMovieItem>("movies", page, cancellationToken);

    /// <summary><c>GET /tvshows/latest/page-{page}.json</c>.</summary>
    public Task<LatestResult<LatestTvShowItem>?> GetLatestTvShowsAsync(int page = 1, CancellationToken cancellationToken = default)
        => GetLatestAsync<LatestTvShowItem>("tvshows", page, cancellationToken);

    /// <summary><c>GET /episodes/latest/page-{page}.json</c>.</summary>
    public Task<LatestResult<LatestEpisodeItem>?> GetLatestEpisodesAsync(int page = 1, CancellationToken cancellationToken = default)
        => GetLatestAsync<LatestEpisodeItem>("episodes", page, cancellationToken);

    /// <summary>Streams every page of a <c>latest</c> endpoint until the last page.</summary>
    public async IAsyncEnumerable<LatestMovieItem> GetAllLatestMoviesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in EnumerateLatestAsync(GetLatestMoviesAsync, cancellationToken))
            yield return item;
    }

    /// <summary>Streams every page of the latest TV shows endpoint.</summary>
    public async IAsyncEnumerable<LatestTvShowItem> GetAllLatestTvShowsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in EnumerateLatestAsync(GetLatestTvShowsAsync, cancellationToken))
            yield return item;
    }

    /// <summary>Streams every page of the latest episodes endpoint.</summary>
    public async IAsyncEnumerable<LatestEpisodeItem> GetAllLatestEpisodesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in EnumerateLatestAsync(GetLatestEpisodesAsync, cancellationToken))
            yield return item;
    }

    // ── ID lists (plain text, one id per line, updated daily) ───────────────

    /// <summary><c>GET /ids/movie_imdb.txt</c> — IMDB ids of every movie (<c>tt1234567</c>).</summary>
    public Task<IReadOnlyList<string>> GetMovieImdbIdsAsync(CancellationToken cancellationToken = default)
        => GetIdListAsync("ids/movie_imdb.txt", cancellationToken);

    /// <summary><c>GET /ids/movie_tmdb.txt</c> — TMDB ids of every movie.</summary>
    public Task<IReadOnlyList<string>> GetMovieTmdbIdsAsync(CancellationToken cancellationToken = default)
        => GetIdListAsync("ids/movie_tmdb.txt", cancellationToken);

    /// <summary><c>GET /ids/tv_imdb.txt</c> — IMDB ids of every TV show.</summary>
    public Task<IReadOnlyList<string>> GetTvShowImdbIdsAsync(CancellationToken cancellationToken = default)
        => GetIdListAsync("ids/tv_imdb.txt", cancellationToken);

    /// <summary><c>GET /ids/tv_tmdb.txt</c> — TMDB ids of every TV show.</summary>
    public Task<IReadOnlyList<string>> GetTvShowTmdbIdsAsync(CancellationToken cancellationToken = default)
        => GetIdListAsync("ids/tv_tmdb.txt", cancellationToken);

    /// <summary><c>GET /ids/eps_tmdb.txt</c> — TMDB ids of every episode, formatted <c>{tmdbId}_{season}x{episode}</c>.</summary>
    public Task<IReadOnlyList<string>> GetEpisodeTmdbIdsAsync(CancellationToken cancellationToken = default)
        => GetIdListAsync("ids/eps_tmdb.txt", cancellationToken);

    /// <summary><c>GET /ids/eps_imdb.txt</c> — IMDB ids of every episode, formatted <c>{imdbId}_{season}x{episode}</c>.</summary>
    public Task<IReadOnlyList<string>> GetEpisodeImdbIdsAsync(CancellationToken cancellationToken = default)
        => GetIdListAsync("ids/eps_imdb.txt", cancellationToken);

    // ── Internals ───────────────────────────────────────────────────────────

    private async Task<T?> GetJsonAsync<T>(string relativeUrl, bool allowNotFound, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(relativeUrl, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound && allowNotFound)
        {
            // VidSrc returns a JSON body on 404 too; deserialize it so callers can read the message.
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        if (!response.IsSuccessStatusCode)
            throw new VidSrcApiException(response.StatusCode, relativeUrl);

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private Task<LatestResult<T>?> GetLatestAsync<T>(string category, int page, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        return GetJsonAsync<LatestResult<T>>($"{category}/latest/page-{page}.json", allowNotFound: false, cancellationToken);
    }

    private static async IAsyncEnumerable<T> EnumerateLatestAsync<T>(
        Func<int, CancellationToken, Task<LatestResult<T>?>> fetchPage,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var page = 1; ; page++)
        {
            var result = await fetchPage(page, cancellationToken).ConfigureAwait(false);
            if (result is null || result.Result.Count == 0)
                yield break;

            foreach (var item in result.Result)
                yield return item;

            if (page >= result.Pages)
                yield break;
        }
    }

    private async Task<IReadOnlyList<string>> GetIdListAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(relativeUrl, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new VidSrcApiException(response.StatusCode, relativeUrl);

        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
